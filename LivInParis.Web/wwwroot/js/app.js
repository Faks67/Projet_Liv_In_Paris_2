// Point d'entrée : navigation, en-tête, panier
import { esc, prix, visuelPlat, vide } from './outils.js';
import * as etat from './etat.js';
import { menuCompte, demanderConnexion } from './vues/compte.js';
import { vueCatalogue } from './vues/catalogue.js';
import { vueCommander } from './vues/commander.js';
import { vueCommandes } from './vues/commandes.js';
import { vueMetro } from './vues/metro.js';
import { vueCuisinier } from './vues/cuisinier.js';
import { vueAdmin } from './vues/admin.js';
import { vueProfil } from './vues/compte.js';

const routes = {
  '': { vue: vueCatalogue, titre: 'Catalogue' },
  'commander': { vue: vueCommander, titre: 'Commander' },
  'commandes': { vue: vueCommandes, titre: 'Mes commandes', role: 'client' },
  'metro': { vue: vueMetro, titre: 'Plan du métro' },
  'cuisine': { vue: vueCuisinier, titre: 'Ma cuisine', role: 'cuisinier' },
  'admin': { vue: vueAdmin, titre: 'Administration', role: 'admin' },
  'profil': { vue: vueProfil, titre: 'Mon profil', role: 'connecte' },
};

function aLeRole(role) {
  const s = etat.session;
  if (!role) return true;
  if (role === 'connecte') return s.connecte;
  if (role === 'admin') return s.admin;
  return Boolean(s[role]);
}

function dessinerNav() {
  const actuelle = location.hash.replace(/^#\/?/, '').split('/')[0];
  const liens = [
    ['', 'Plats'],
    ['metro', 'Plan du métro'],
    ['commandes', 'Mes commandes', 'client'],
    ['cuisine', 'Ma cuisine', 'cuisinier'],
    ['admin', 'Administration', 'admin'],
  ].filter(([, , role]) => aLeRole(role));
  document.getElementById('nav').innerHTML = liens
    .map(([route, libelle]) => `<a href="#/${route}" class="${route === actuelle ? 'actif' : ''}">${libelle}</a>`).join('');
  document.getElementById('zone-compte').innerHTML = menuCompte();
}

let nettoyage = null;
async function router() {
  const [nom, ...params] = location.hash.replace(/^#\/?/, '').split('/');
  const route = routes[nom] ?? routes[''];
  document.getElementById('nav').classList.remove('ouvert');
  dessinerNav();
  document.title = `${route.titre} · Liv'In Paris`;
  const conteneur = document.getElementById('vue');
  if (typeof nettoyage === 'function') nettoyage();
  nettoyage = null;

  if (!aLeRole(route.role)) {
    if (!etat.session.connecte) {
      conteneur.innerHTML = vide('Connexion requise', 'Connectez-vous pour accéder à cette page.',
        '<button class="btn btn-primaire" id="btn-connexion-requise">Se connecter</button>');
      document.getElementById('btn-connexion-requise').onclick = () => demanderConnexion(router);
    } else {
      conteneur.innerHTML = vide('Accès réservé', "Votre compte n'a pas accès à cette page.", '<a class="btn" href="#/">Retour aux plats</a>');
    }
    return;
  }
  conteneur.innerHTML = '<div class="chargement">Chargement…</div>';
  try {
    nettoyage = await route.vue(conteneur, params);
  } catch (e) {
    conteneur.innerHTML = vide('Oups', e.message);
  }
  window.scrollTo(0, 0);
}

// ---------- Panier ----------
function ouvrirPanier(ouvert = true) {
  document.getElementById('tiroir').classList.toggle('ouvert', ouvert);
  document.getElementById('voile').classList.toggle('ouvert', ouvert);
}

function dessinerPanier() {
  const n = etat.nombreArticles();
  const compteur = document.getElementById('compteur-panier');
  compteur.textContent = n;
  compteur.classList.toggle('hidden', n === 0);

  const contenu = document.getElementById('contenu-panier');
  const pied = document.getElementById('pied-panier');
  if (!etat.panier.length) {
    contenu.innerHTML = vide('Panier vide', 'Ajoutez des plats depuis le catalogue.');
    pied.innerHTML = '';
    return;
  }
  contenu.innerHTML = etat.panier.map(l => `
    <div class="ligne-panier">
      <div class="mini-visuel" style="background:${visuelPlat(l)}">${esc(l.nom[0])}</div>
      <div><strong>${esc(l.nom)}</strong><div class="muted small">${esc(l.cuisinier)} · ${prix(l.prix)}</div></div>
      <div class="pile" style="gap:4px;align-items:end">
        <div class="quantite"><button data-moins="${esc(l.id)}" aria-label="Retirer">−</button><span>${l.portions}</span><button data-plus="${esc(l.id)}" aria-label="Ajouter">+</button></div>
        <span class="small muted">${prix(l.prix * l.portions)}</span>
      </div>
    </div>`).join('');
  pied.innerHTML = `
    <div class="total"><span>Total</span><span>${prix(etat.totalPanier())}</span></div>
    <a class="btn btn-primaire btn-large" href="#/commander" id="aller-commander">Choisir l'adresse de livraison</a>`;
  document.getElementById('aller-commander').onclick = () => ouvrirPanier(false);
}

document.getElementById('contenu-panier').addEventListener('click', e => {
  const moins = e.target.closest('[data-moins]');
  const plus = e.target.closest('[data-plus]');
  if (moins) etat.changerPortions(moins.dataset.moins, -1);
  if (plus) etat.changerPortions(plus.dataset.plus, 1);
});
document.getElementById('btn-panier').onclick = () => ouvrirPanier(true);
document.getElementById('fermer-panier').onclick = () => ouvrirPanier(false);
document.getElementById('voile').onclick = () => ouvrirPanier(false);
document.getElementById('fermer-modale').onclick = () => etat.fermerModale();
document.getElementById('btn-menu').onclick = () => document.getElementById('nav').classList.toggle('ouvert');

// ---------- Thème ----------
const CLE_THEME = 'livinparis.theme';
try {
  const theme = localStorage.getItem(CLE_THEME);
  if (theme) document.documentElement.dataset.theme = theme;
} catch { /* stockage indisponible */ }
document.getElementById('btn-theme').onclick = () => {
  const sombre = matchMedia('(prefers-color-scheme: dark)').matches;
  const actuel = document.documentElement.dataset.theme || (sombre ? 'dark' : 'light');
  const nouveau = actuel === 'dark' ? 'light' : 'dark';
  document.documentElement.dataset.theme = nouveau;
  try { localStorage.setItem(CLE_THEME, nouveau); } catch { /* stockage indisponible */ }
  router();
};

etat.surChangement(() => { dessinerPanier(); dessinerNav(); });
window.addEventListener('hashchange', router);
window.addEventListener('livinparis:ouvrir-panier', () => ouvrirPanier(true));
window.addEventListener('livinparis:session', router);

dessinerPanier();
await etat.rafraichirSession();
router();
