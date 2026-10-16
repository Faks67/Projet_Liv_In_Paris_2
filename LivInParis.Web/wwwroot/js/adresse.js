// Champ d'adresse avec suggestions (Géoplateforme IGN, ex-API Adresse du gouvernement)
// et suggestions de stations de métro
import { api, esc } from './outils.js';

const SERVICES = [
  q => `https://data.geopf.fr/geocodage/search?q=${encodeURIComponent(q)}&limit=15&lat=48.8566&lon=2.3522`,
  q => `https://api-adresse.data.gouv.fr/search/?q=${encodeURIComponent(q)}&limit=15&lat=48.8566&lon=2.3522`
];

let stationsCache = null;
async function stations() {
  stationsCache ??= await api('/api/metro/stations');
  return stationsCache;
}

const DEPARTEMENTS_IDF = new Set(['75', '77', '78', '91', '92', '93', '94', '95']);

const normaliser = s => s.normalize('NFD').replace(/[̀-ͯ]/g, '').toLowerCase().replace(/[^a-z0-9]/g, '');

async function chercherAdresses(q) {
  for (const url of SERVICES) {
    try {
      const reponse = await fetch(url(q));
      if (!reponse.ok) continue;
      const json = await reponse.json();
      const resultats = json.features.map(f => ({
        libelle: f.properties.label,
        detail: f.properties.context,
        codePostal: f.properties.postcode || f.properties.citycode || '',
        lat: f.geometry.coordinates[1],
        lon: f.geometry.coordinates[0]
      }));
      // Livraison en Île-de-France uniquement : on écarte les homonymes de province
      const idf = resultats.filter(r => DEPARTEMENTS_IDF.has(String(r.codePostal).slice(0, 2)));
      return (idf.length ? idf : resultats).slice(0, 6);
    } catch { /* service suivant */ }
  }
  return [];
}

/**
 * Transforme un <input> en champ d'adresse. onChoix reçoit { libelle, lat, lon, station? }.
 * Avec { stations: true }, les stations de métro sont aussi proposées.
 */
export function champAdresse(input, onChoix, { stations: avecStations = false } = {}) {
  const conteneur = document.createElement('div');
  conteneur.className = 'auto';
  input.replaceWith(conteneur);
  conteneur.append(input);
  input.autocomplete = 'off';
  const liste = document.createElement('div');
  liste.className = 'auto-liste hidden';
  conteneur.append(liste);

  let resultats = [];
  let actif = -1;
  let minuteur = null;
  let requete = 0;
  input.dataset.valide = input.value ? '1' : '';

  const fermer = () => { liste.classList.add('hidden'); actif = -1; };
  const choisir = r => {
    input.value = r.libelle;
    input.dataset.valide = '1';
    fermer();
    onChoix(r);
  };

  const afficher = () => {
    if (!resultats.length) { fermer(); return; }
    liste.innerHTML = resultats.map((r, i) => `
      <div class="auto-item ${i === actif ? 'actif' : ''}" data-i="${i}">
        <span>${r.station ? 'Ⓜ ' : ''}${esc(r.libelle)}</span>${r.detail ? `<small>${esc(r.detail)}</small>` : ''}
      </div>`).join('');
    liste.classList.remove('hidden');
  };

  input.addEventListener('input', () => {
    input.dataset.valide = '';
    clearTimeout(minuteur);
    const q = input.value.trim();
    if (q.length < 3) { fermer(); return; }
    minuteur = setTimeout(async () => {
      const numero = ++requete;
      const [adresses, toutes] = await Promise.all([chercherAdresses(q), avecStations ? stations() : []]);
      if (numero !== requete) return;
      const cle = normaliser(q);
      // Nom exact d'abord, puis début de nom, puis le reste
      const rang = s => { const n = normaliser(s.nom); return n === cle ? 0 : n.startsWith(cle) ? 1 : 2; };
      const gares = toutes.filter(s => normaliser(s.nom).includes(cle)).sort((a, b) => rang(a) - rang(b)).slice(0, 4)
        .map(s => ({ libelle: s.nom, detail: `Métro ${s.lignes.join(', ')}`, lat: s.lat, lon: s.lon, station: s.id }));
      resultats = [...gares, ...adresses];
      actif = -1;
      afficher();
    }, 220);
  });

  input.addEventListener('keydown', e => {
    if (liste.classList.contains('hidden')) return;
    if (e.key === 'ArrowDown') { actif = Math.min(actif + 1, resultats.length - 1); afficher(); e.preventDefault(); }
    else if (e.key === 'ArrowUp') { actif = Math.max(actif - 1, 0); afficher(); e.preventDefault(); }
    else if (e.key === 'Enter' && actif >= 0) { choisir(resultats[actif]); e.preventDefault(); }
    else if (e.key === 'Escape') fermer();
  });

  liste.addEventListener('mousedown', e => {
    const item = e.target.closest('.auto-item');
    if (item) { e.preventDefault(); choisir(resultats[Number(item.dataset.i)]); }
  });
  input.addEventListener('blur', () => setTimeout(fermer, 150));
}
