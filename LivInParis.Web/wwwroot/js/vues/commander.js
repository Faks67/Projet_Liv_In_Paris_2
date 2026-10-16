// Validation du panier : adresse de livraison, itinéraires depuis chaque cuisinier, confirmation
import { api, esc, prix, minutes, visuelPlat, vide, toast } from '../outils.js';
import * as etat from '../etat.js';
import { champAdresse } from '../adresse.js';
import { creerCarte, dessinerTrajet, feuilleDeRoute } from '../carte.js';
import { demanderConnexion } from './compte.js';

const normaliser = s => (s || '').normalize('NFD').replace(/[̀-ͯ]/g, '').toLowerCase().replace(/[^a-z0-9]/g, '');

export async function vueCommander(conteneur) {
  if (!etat.panier.length) {
    conteneur.innerHTML = vide('Votre panier est vide', 'Choisissez des plats avant de commander.', '<a class="btn btn-primaire" href="#/">Voir les plats</a>');
    return;
  }
  const stations = await api('/api/metro/stations');
  let adresse = null;
  let trajets = [];
  let calque = null;

  conteneur.innerHTML = `
    <div class="titre-section"><div><h1>Livraison</h1><p class="muted">Indiquez où livrer : on calcule le trajet en métro depuis chaque cuisinier.</p></div></div>
    <div class="validation">
      <div class="pile">
        <div class="carte"><div class="carte-corps pile">
          <h3>Adresse de livraison</h3>
          <input id="adresse" placeholder="Ex : 5 place de la République, Paris">
          <p class="muted small" id="info-adresse">Commencez à taper puis choisissez une adresse dans la liste.</p>
        </div></div>
        <div class="carte"><div class="carte-corps pile" id="recap"></div></div>
        <div class="carte hidden" id="bloc-trajets"><div class="carte-corps pile" id="trajets"></div></div>
      </div>
      <div class="carte-map haute" id="carte"></div>
    </div>`;

  const carte = creerCarte(conteneur.querySelector('#carte'));
  const recap = conteneur.querySelector('#recap');

  // Une livraison par cuisinier (chacun part de sa station)
  const groupes = () => Object.values(etat.panier.reduce((g, l) => {
    (g[l.cuisinier] ??= { cuisinier: l.cuisinier, station: l.station, lignes: [] }).lignes.push(l);
    return g;
  }, {}));

  const dessinerRecap = () => {
    const s = etat.session;
    const duree = trajets.length ? Math.max(...trajets.map(t => t.trajet?.totalMinutes ?? 0)) : null;
    let bouton;
    if (!s.connecte) bouton = '<button class="btn btn-primaire btn-large" id="valider">Se connecter pour commander</button>';
    else if (!s.client) bouton = `<button class="btn btn-primaire btn-large" id="valider">${s.admin ? 'Le compte admin ne peut pas commander' : 'Créer mon profil client'}</button>`;
    else bouton = `<button class="btn btn-primaire btn-large" id="valider" ${adresse ? '' : 'disabled'}>Commander · ${prix(etat.totalPanier())}</button>`;

    recap.innerHTML = `
      <h3>Votre commande</h3>
      ${groupes().map(g => `
        <div class="pile" style="gap:8px">
          <p class="small muted">Cuisiné par <strong>${esc(g.cuisinier)}</strong> · Ⓜ ${esc(g.station)}</p>
          ${g.lignes.map(l => `
            <div class="ligne-panier">
              <div class="mini-visuel" style="background:${visuelPlat(l)}">${esc(l.nom[0])}</div>
              <div><strong>${esc(l.nom)}</strong><div class="muted small">${l.portions} × ${prix(l.prix)}</div></div>
              <strong>${prix(l.prix * l.portions)}</strong>
            </div>`).join('')}
        </div>`).join('<hr style="border:0;border-top:1px solid var(--border);width:100%">')}
      <div class="total"><span>Total</span><span>${prix(etat.totalPanier())}</span></div>
      ${duree !== null ? `<p class="muted small">Livraison estimée en <strong>${minutes(duree)}</strong> après préparation.</p>` : ''}
      ${bouton}`;

    const valider = recap.querySelector('#valider');
    if (!s.connecte) valider.onclick = () => demanderConnexion(dessinerRecap);
    else if (!s.client) valider.onclick = () => { if (!s.admin) location.hash = '#/profil'; };
    else valider.onclick = commander;
  };

  const calculer = async () => {
    const bloc = conteneur.querySelector('#bloc-trajets');
    const zone = conteneur.querySelector('#trajets');
    bloc.classList.remove('hidden');
    zone.innerHTML = '<p class="muted">Calcul des itinéraires…</p>';
    trajets = await Promise.all(groupes().map(async g => {
      const station = stations.find(s => normaliser(s.nom) === normaliser(g.station));
      if (!station) return { ...g, trajet: null };
      const params = new URLSearchParams({ de: station.id, versLat: adresse.lat, versLon: adresse.lon, versNom: adresse.libelle });
      try {
        return { ...g, trajet: await api(`/api/metro/trajet?${params}`) };
      } catch {
        return { ...g, trajet: null };
      }
    }));
    zone.innerHTML = `<h3>Trajets de livraison</h3>` + trajets.map((t, i) => `
      <details ${i === 0 ? 'open' : ''} data-i="${i}">
        <summary style="cursor:pointer;font-weight:600;padding:6px 0">${esc(t.cuisinier)} · ${t.trajet ? minutes(t.trajet.totalMinutes) : 'trajet indisponible'}</summary>
        ${t.trajet ? feuilleDeRoute(t.trajet) : '<p class="muted small">La station du cuisinier est inconnue du plan.</p>'}
      </details>`).join('');
    zone.querySelectorAll('details').forEach(d => d.addEventListener('toggle', () => {
      if (d.open && trajets[d.dataset.i].trajet) calque = dessinerTrajet(carte, trajets[d.dataset.i].trajet, calque, { libelleDepart: 'C', libelleArrivee: '⌂' });
    }));
    const premier = trajets.find(t => t.trajet);
    if (premier) calque = dessinerTrajet(carte, premier.trajet, calque, { libelleDepart: 'C', libelleArrivee: '⌂' });
    dessinerRecap();
  };

  const choisirAdresse = async a => {
    adresse = a;
    const { station, minutesAPied } = await api(`/api/metro/proche?lat=${a.lat}&lon=${a.lon}`);
    conteneur.querySelector('#info-adresse').textContent = `Station la plus proche : ${station.nom} (${minutesAPied} min à pied)`;
    await calculer();
  };

  async function commander() {
    const bouton = recap.querySelector('#valider');
    bouton.disabled = true;
    try {
      const { commandes } = await api('/api/commandes', {
        corps: { articles: etat.panier.map(l => ({ idPlat: l.id, portions: l.portions })), adresse: adresse.libelle, lat: adresse.lat, lon: adresse.lon }
      });
      etat.viderPanier();
      toast(commandes.length > 1 ? `${commandes.length} commandes envoyées aux cuisiniers !` : 'Commande envoyée au cuisinier !');
      location.hash = '#/commandes';
    } catch (err) {
      toast(err.message, true);
      bouton.disabled = false;
    }
  }

  const input = conteneur.querySelector('#adresse');
  champAdresse(input, choisirAdresse);
  dessinerRecap();

  // Adresse du profil client proposée par défaut
  const adresseProfil = etat.session.client?.adresse;
  if (adresseProfil && adresseProfil.length > 5) {
    input.value = adresseProfil;
    try {
      const r = await fetch(`https://data.geopf.fr/geocodage/search?q=${encodeURIComponent(adresseProfil)}&limit=1&lat=48.8566&lon=2.3522`);
      const f = (await r.json()).features?.[0];
      // On ne retient l'adresse du profil que si elle est reconnue sans ambiguïté
      if (f && f.properties.score > 0.85) {
        input.value = f.properties.label;
        input.dataset.valide = '1';
        await choisirAdresse({ libelle: f.properties.label, lat: f.geometry.coordinates[1], lon: f.geometry.coordinates[0] });
      }
    } catch { /* l'utilisateur saisira l'adresse */ }
  }
  return () => carte.remove();
}
