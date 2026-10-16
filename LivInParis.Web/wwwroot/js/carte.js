// Cartes Leaflet : fond de carte, plan du métro et tracé des itinéraires
import { api, esc, minutes, pastilleLigne, COULEURS_LIGNES } from './outils.js';

const PARIS = [48.8566, 2.3522];

function sombre() {
  const theme = document.documentElement.dataset.theme;
  return theme === 'dark' || (!theme && matchMedia('(prefers-color-scheme: dark)').matches);
}

export function creerCarte(element, { zoom = 12 } = {}) {
  const carte = L.map(element, { zoomControl: true, scrollWheelZoom: true }).setView(PARIS, zoom);
  L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
    attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>',
    maxZoom: 19,
    // Fond atténué (et assombri en mode sombre) pour faire ressortir les lignes de métro
    className: sombre() ? 'fond-carte fond-sombre' : 'fond-carte'
  }).addTo(carte);
  // La carte peut être créée dans un élément qui vient d'apparaître
  setTimeout(() => carte.invalidateSize(), 50);
  return carte;
}

let planCache = null;
export async function plan() {
  planCache ??= await api('/api/metro/plan');
  return planCache;
}

/**
 * Dessine tout le réseau en arrière-plan (lignes fines aux couleurs officielles)
 */
export async function dessinerReseau(carte, { opacite = 0.55, stationsCliquables = null } = {}) {
  const { segments, stations } = await plan();
  const calque = L.layerGroup().addTo(carte);
  for (const s of segments) {
    L.polyline([s.de, s.vers], { color: COULEURS_LIGNES[s.ligne] || '#888', weight: 3, opacity: opacite }).addTo(calque);
  }
  const vues = new Set();
  for (const s of stations) {
    if (vues.has(s.nom)) continue;
    vues.add(s.nom);
    const point = L.circleMarker([s.lat, s.lon], { radius: 3, color: '#555', weight: 1, fillColor: '#fff', fillOpacity: 1 }).addTo(calque);
    point.bindTooltip(esc(s.nom), { direction: 'top' });
    if (stationsCliquables) point.on('click', () => stationsCliquables(s));
  }
  return calque;
}

function marqueur(lat, lon, texte, couleur) {
  return L.marker([lat, lon], {
    icon: L.divIcon({ className: '', html: `<div class="marqueur" style="background:${couleur}">${texte}</div>`, iconSize: [34, 34], iconAnchor: [17, 17] })
  });
}

/**
 * Trace un trajet porte à porte renvoyé par l'API : marche (pointillés) + tronçons de métro colorés
 */
export function dessinerTrajet(carte, trajet, calquePrecedent, { libelleDepart = 'A', libelleArrivee = 'B' } = {}) {
  calquePrecedent?.remove();
  const calque = L.layerGroup().addTo(carte);
  const points = [];
  const marche = { color: '#6b665b', weight: 4, dashArray: '2 8', lineCap: 'round' };
  const depart = [trajet.depart.lat, trajet.depart.lon];
  const arrivee = [trajet.arrivee.lat, trajet.arrivee.lon];

  if (trajet.metro) {
    const etapes = trajet.metro.etapes;
    const premier = [etapes[0].lat, etapes[0].lon];
    const dernier = [etapes.at(-1).lat, etapes.at(-1).lon];
    L.polyline([depart, premier], marche).addTo(calque);
    L.polyline([dernier, arrivee], marche).addTo(calque);
    for (let i = 1; i < etapes.length; i++) {
      const a = etapes[i - 1], b = etapes[i];
      const correspondance = a.nom === b.nom;
      L.polyline([[a.lat, a.lon], [b.lat, b.lon]], correspondance ? marche
        : { color: COULEURS_LIGNES[b.ligne] || '#888', weight: 7, opacity: .95, lineCap: 'round' }).addTo(calque);
    }
    etapes.forEach((e, i) => {
      const extremite = i === 0 || i === etapes.length - 1;
      L.circleMarker([e.lat, e.lon], { radius: extremite ? 6 : 4, color: '#1d1b16', weight: 2, fillColor: '#fff', fillOpacity: 1 })
        .bindTooltip(esc(e.nom)).addTo(calque);
      points.push([e.lat, e.lon]);
    });
  } else {
    L.polyline([depart, arrivee], marche).addTo(calque);
  }

  marqueur(...depart, libelleDepart, '#1d1b16').bindPopup(esc(trajet.depart.libelle)).addTo(calque);
  marqueur(...arrivee, libelleArrivee, 'var(--accent, #d9480f)').bindPopup(esc(trajet.arrivee.libelle)).addTo(calque);
  points.push(depart, arrivee);
  carte.fitBounds(L.latLngBounds(points), { padding: [40, 40], maxZoom: 15 });
  return calque;
}

/**
 * Feuille de route lisible : marche, lignes empruntées, correspondances
 */
export function feuilleDeRoute(trajet) {
  const lignes = [];
  const etape = (icone, titre, detail, duree, trait = '') =>
    `<div class="etape" style="--trait:${trait}"><div>${icone}</div><div><strong>${titre}</strong>${detail ? `<div class="muted small">${detail}</div>` : ''}</div><div class="muted small">${duree}</div></div>`;
  const pied = `<span class="etape-icone" title="À pied"><svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
    <circle cx="13" cy="4" r="2"/><path d="M10 22l2-7 3 3v6"/><path d="M8 12l2-4 4 1 3 3"/><path d="M12 9l-1 6"/></svg></span>`;

  if (!trajet.metro) {
    lignes.push(etape(pied, `À pied jusqu'à ${esc(trajet.arrivee.libelle)}`, 'Plus rapide que le métro', minutes(trajet.marcheDepart)));
  } else {
    if (trajet.marcheDepart >= 1)
      lignes.push(etape(pied, `Marcher jusqu'à ${esc(trajet.stationDepart.nom)}`, `Depuis ${esc(trajet.depart.libelle)}`, minutes(trajet.marcheDepart)));
    trajet.metro.troncons.forEach((t, i) => {
      if (i > 0) lignes.push(etape('<span class="etape-icone">⇄</span>', `Correspondance à ${esc(t.depart)}`, '', ''));
      lignes.push(etape(pastilleLigne(t.ligne), `Ligne ${esc(t.ligne)} : ${esc(t.depart)} → ${esc(t.arrivee)}`,
        `${t.nbArrets} arrêt${t.nbArrets > 1 ? 's' : ''}`, minutes(t.minutes), COULEURS_LIGNES[t.ligne]));
    });
    if (trajet.marcheArrivee >= 1)
      lignes.push(etape(pied, `Marcher jusqu'à l'arrivée`, esc(trajet.arrivee.libelle), minutes(trajet.marcheArrivee)));
  }
  const correspondances = trajet.metro?.correspondances ?? 0;
  return `
    <div class="resume-trajet"><strong>${minutes(trajet.totalMinutes)}</strong>
      <span class="muted">${trajet.metro ? `${trajet.metro.troncons.length} ligne${trajet.metro.troncons.length > 1 ? 's' : ''}, ${correspondances} correspondance${correspondances > 1 ? 's' : ''}` : 'à pied'}</span></div>
    <div class="itineraire" style="margin-top:12px">${lignes.join('')}</div>`;
}
