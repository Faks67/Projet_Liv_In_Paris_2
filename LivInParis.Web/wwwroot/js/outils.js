// Petites fonctions partagées : appels API, échappement HTML, formats, notifications

export async function api(url, options = {}) {
  const init = { credentials: 'same-origin', ...options, headers: { ...(options.headers || {}) } };
  if (options.corps !== undefined) {
    init.method = init.method || 'POST';
    init.headers['Content-Type'] = 'application/json';
    init.body = JSON.stringify(options.corps);
  }
  const reponse = await fetch(url, init);
  const texte = await reponse.text();
  const donnees = texte ? JSON.parse(texte) : null;
  if (!reponse.ok) {
    const message = donnees?.erreur
      || (reponse.status === 401 ? 'Connectez-vous pour continuer.' : reponse.status === 403 ? 'Accès réservé.' : 'Une erreur est survenue.');
    const erreur = new Error(message);
    erreur.status = reponse.status;
    throw erreur;
  }
  return donnees;
}

export function esc(valeur) {
  return String(valeur ?? '').replace(/[&<>"']/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
}

const euros = new Intl.NumberFormat('fr-FR', { style: 'currency', currency: 'EUR' });
export const prix = v => euros.format(Number(v) || 0);

export function date(valeur, avecHeure = false) {
  if (!valeur) return '';
  const d = new Date(valeur);
  return d.toLocaleDateString('fr-FR', avecHeure
    ? { day: 'numeric', month: 'short', hour: '2-digit', minute: '2-digit' }
    : { day: 'numeric', month: 'short', year: 'numeric' });
}

export function minutes(m) {
  m = Math.round(m);
  if (m < 60) return `${m} min`;
  return `${Math.floor(m / 60)} h ${String(m % 60).padStart(2, '0')}`;
}

export function toast(message, erreur = false) {
  const el = document.createElement('div');
  el.className = 'toast' + (erreur ? ' erreur' : '');
  el.textContent = message;
  document.getElementById('toasts').append(el);
  setTimeout(() => el.remove(), 3500);
}

// Couleurs officielles des lignes de métro
export const COULEURS_LIGNES = {
  '1': '#FFCD00', '2': '#003CA6', '3': '#837902', '3bis': '#6EC4E8', '4': '#CF009E', '5': '#FF7E2E',
  '6': '#6ECA97', '7': '#FA9ABA', '7bis': '#6ECA97', '8': '#E19BDF', '9': '#B6BD00', '10': '#C9910D',
  '11': '#704B1C', '12': '#007852', '13': '#6EC4E8', '14': '#62259D'
};
const TEXTE_FONCE = new Set(['1', '3bis', '5', '6', '7', '7bis', '8', '9', '10', '13']);

export function pastilleLigne(ligne) {
  const fond = COULEURS_LIGNES[ligne] || '#888';
  const texte = TEXTE_FONCE.has(ligne) ? '#1d1b16' : '#fff';
  return `<span class="pastille-ligne" style="background:${fond};color:${texte}" title="Ligne ${esc(ligne)}">${esc(ligne.replace('bis', 'b'))}</span>`;
}

// Visuel d'un plat : un dégradé dérivé de sa nationalité et l'initiale de son nom
const TEINTES = { italienne: 8, japonaise: 350, americaine: 20, 'américaine': 20, maghrebine: 35, 'maghrébine': 35, espagnole: 45, francaise: 220, 'française': 220, indienne: 30, chinoise: 0, libanaise: 140, mexicaine: 100 };
export function visuelPlat(plat) {
  const cle = (plat.nationalite || '').toLowerCase();
  let teinte = TEINTES[cle];
  if (teinte === undefined) teinte = [...cle].reduce((h, c) => (h * 31 + c.charCodeAt(0)) % 360, 17);
  return `linear-gradient(135deg, hsl(${teinte} 70% 52%), hsl(${(teinte + 35) % 360} 75% 38%))`;
}

export function badgeStatut(statut) {
  const classes = { 'En cours': 'badge-info', 'En livraison': 'badge-warn', 'Livrée': 'badge-ok', 'Annulée': 'badge-danger' };
  return `<span class="badge ${classes[statut] || ''}">${esc(statut)}</span>`;
}

export function vide(titre, texte, action = '') {
  return `<div class="vide"><h3>${esc(titre)}</h3><p>${esc(texte)}</p>${action ? `<div style="margin-top:16px">${action}</div>` : ''}</div>`;
}
