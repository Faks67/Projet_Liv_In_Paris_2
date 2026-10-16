// Suivi des commandes du client
import { api, esc, prix, date, minutes, badgeStatut, vide, toast } from '../outils.js';
import * as etat from '../etat.js';
import { creerCarte, dessinerTrajet, feuilleDeRoute } from '../carte.js';

const ETAPES = ['En cours', 'En livraison', 'Livrée'];

export async function vueCommandes(conteneur) {
  const dessiner = async () => {
    const commandes = await api('/api/commandes/mes');
    const enCours = commandes.filter(c => c.statut === 'En cours' || c.statut === 'En livraison');
    const passees = commandes.filter(c => !enCours.includes(c));
    conteneur.innerHTML = `
      <div class="titre-section"><div><h1>Mes commandes</h1><p class="muted">Suivez vos livraisons en temps réel.</p></div>
        <a class="btn" href="#/">Commander à nouveau</a></div>
      ${!commandes.length ? vide('Aucune commande', 'Vos commandes apparaîtront ici.', '<a class="btn btn-primaire" href="#/">Voir les plats</a>') : ''}
      ${enCours.length ? `<h2 style="margin:8px 0 14px">En cours</h2><div class="carte">${enCours.map(ligne).join('')}</div>` : ''}
      ${passees.length ? `<h2 style="margin:28px 0 14px">Historique</h2><div class="carte">${passees.map(ligne).join('')}</div>` : ''}`;
  };

  const clic = async e => {
    const trajet = e.target.closest('[data-trajet]');
    const annuler = e.target.closest('[data-annuler]');
    if (trajet) afficherTrajet(trajet.dataset.trajet);
    if (annuler && confirm('Annuler cette commande ?')) {
      try {
        await api(`/api/commandes/${encodeURIComponent(annuler.dataset.annuler)}/annuler`, { method: 'POST' });
        toast('Commande annulée.');
        await dessiner();
      } catch (err) { toast(err.message, true); }
    }
  };
  conteneur.addEventListener('click', clic);

  await dessiner();
  // Les cuisiniers font avancer les commandes : on rafraîchit régulièrement
  const minuteur = setInterval(() => { if (!document.hidden && !document.getElementById('modale').open) dessiner().catch(() => {}); }, 20000);
  return () => { clearInterval(minuteur); conteneur.removeEventListener('click', clic); };
}

function ligne(c) {
  const rang = ETAPES.indexOf(c.statut);
  return `
    <div class="commande">
      <div class="pile" style="gap:4px">
        <div class="rangee" style="gap:8px"><strong>${esc(c.plat ?? 'Plat supprimé')}</strong>${badgeStatut(c.statut)}<span class="muted small">${esc(c.id)}</span></div>
        <span class="muted small">${c.portions} portion${c.portions > 1 ? 's' : ''} · cuisiné par ${esc(c.cuisinier)} · ${date(c.date, true)}</span>
        <span class="small">Ⓜ ${esc(c.depart)} → ${esc(c.adresse || c.arrive)}${c.duree ? ` · environ ${minutes(c.duree)}` : ''}</span>
        ${c.statut !== 'Annulée' ? `<div class="suivi" title="${esc(c.statut)}">${ETAPES.map((_, i) => `<span class="${i <= rang ? 'fait' : ''}"></span>`).join('')}</div>` : ''}
      </div>
      <div class="pile" style="gap:8px;align-items:end">
        <span class="prix">${prix(c.prix)}</span>
        <div class="rangee" style="gap:6px">
          ${c.statut === 'En cours' ? `<button class="btn btn-petit btn-danger" data-annuler="${esc(c.id)}">Annuler</button>` : ''}
          <button class="btn btn-petit" data-trajet="${esc(c.id)}">Voir le trajet</button>
        </div>
      </div>
    </div>`;
}

export async function afficherTrajet(id) {
  const corps = etat.ouvrirModale(`Trajet de la commande ${id}`, '<div class="carte-map" id="carte-trajet" style="height:320px"></div><div id="route" style="margin-top:16px"><p class="muted">Calcul…</p></div>');
  const modale = document.getElementById('modale');
  modale.style.width = 'min(760px, calc(100vw - 32px))';
  modale.addEventListener('close', () => { modale.style.width = ''; carte.remove(); }, { once: true });
  const carte = creerCarte(corps.querySelector('#carte-trajet'));
  try {
    const trajet = await api(`/api/commandes/${encodeURIComponent(id)}/trajet`);
    dessinerTrajet(carte, trajet, null, { libelleDepart: 'C', libelleArrivee: '⌂' });
    corps.querySelector('#route').innerHTML = feuilleDeRoute(trajet);
  } catch (err) {
    corps.querySelector('#route').innerHTML = `<p class="muted">${esc(err.message)}</p>`;
  }
}
