// État partagé de l'application : session, panier, modale
import { api } from './outils.js';

const abonnes = new Set();
export const surChangement = fn => abonnes.add(fn);
const notifier = () => abonnes.forEach(fn => fn());

export let session = { connecte: false };

export async function rafraichirSession() {
  try {
    session = await api('/api/session');
  } catch {
    session = { connecte: false };
  }
  notifier();
  return session;
}

// ---------- Panier (gardé dans le navigateur) ----------
const CLE_PANIER = 'livinparis.panier';

function lirePanier() {
  try {
    return JSON.parse(localStorage.getItem(CLE_PANIER)) || [];
  } catch {
    return [];
  }
}

export let panier = lirePanier();

function sauverPanier() {
  try { localStorage.setItem(CLE_PANIER, JSON.stringify(panier)); } catch { /* stockage indisponible */ }
  notifier();
}

export function ajouterAuPanier(plat) {
  const ligne = panier.find(l => l.id === plat.id);
  if (ligne) {
    if (ligne.portions >= plat.stock) return false;
    ligne.portions++;
  } else {
    panier.push({ id: plat.id, nom: plat.nom, prix: plat.prix, nationalite: plat.nationalite, cuisinier: plat.cuisinier, station: plat.station, stock: plat.stock, portions: 1 });
  }
  sauverPanier();
  return true;
}

export function changerPortions(id, delta) {
  const ligne = panier.find(l => l.id === id);
  if (!ligne) return;
  ligne.portions = Math.min(ligne.stock, ligne.portions + delta);
  if (ligne.portions <= 0) panier = panier.filter(l => l.id !== id);
  sauverPanier();
}

export function viderPanier() {
  panier = [];
  sauverPanier();
}

export const totalPanier = () => panier.reduce((s, l) => s + l.prix * l.portions, 0);
export const nombreArticles = () => panier.reduce((s, l) => s + l.portions, 0);

// ---------- Modale ----------
const modale = () => document.getElementById('modale');

export function ouvrirModale(titre, html) {
  document.getElementById('modale-titre').textContent = titre;
  const corps = document.getElementById('modale-corps');
  corps.innerHTML = html;
  if (!modale().open) modale().showModal();
  return corps;
}

export function fermerModale() {
  if (modale().open) modale().close();
}
