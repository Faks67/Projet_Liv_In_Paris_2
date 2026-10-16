// Espace cuisinier : commandes à préparer, plats, statistiques
import { api, esc, prix, date, minutes, badgeStatut, visuelPlat, vide, toast } from '../outils.js';
import * as etat from '../etat.js';
import { afficherTrajet } from './commandes.js';

const ACTIONS = {
  'En cours': [['En livraison', 'Partir livrer', 'btn-primaire'], ['Annulée', 'Refuser', 'btn-danger']],
  'En livraison': [['Livrée', 'Marquer livrée', 'btn-primaire']],
};

export async function vueCuisinier(conteneur, [ongletInitial]) {
  let onglet = ongletInitial || 'commandes';
  const moi = etat.session.cuisinier;

  conteneur.innerHTML = `
    <div class="titre-section"><div><h1>Ma cuisine</h1><p class="muted">${esc(moi.prenom)} ${esc(moi.nom)} · Ⓜ ${esc(moi.station)}</p></div>
      <button class="btn btn-primaire" id="nouveau-plat">+ Nouveau plat</button></div>
    <div class="kpis" id="kpis"></div>
    <div class="onglets">
      <button data-onglet="commandes">Commandes</button><button data-onglet="plats">Mes plats</button><button data-onglet="clients">Mes clients</button>
    </div>
    <div id="contenu"></div>`;

  const contenu = conteneur.querySelector('#contenu');

  const kpis = async () => {
    const t = await api('/api/cuisinier/tableau');
    conteneur.querySelector('#kpis').innerHTML = `
      <div class="carte kpi"><span class="muted small">À préparer / livrer</span><strong>${t.aPreparer}</strong></div>
      <div class="carte kpi"><span class="muted small">Commandes</span><strong>${t.commandes}</strong></div>
      <div class="carte kpi"><span class="muted small">Chiffre d'affaires</span><strong>${prix(t.chiffreAffaires)}</strong></div>
      <div class="carte kpi"><span class="muted small">Clients servis</span><strong>${t.clients}</strong></div>`;
    return t;
  };

  const vues = {
    async commandes() {
      const commandes = await api('/api/cuisinier/commandes');
      if (!commandes.length) return vide('Pas encore de commande', 'Vos commandes apparaîtront ici dès qu\'un client choisira un de vos plats.');
      return `<div class="carte">${commandes.map(c => `
        <div class="commande">
          <div class="pile" style="gap:4px">
            <div class="rangee" style="gap:8px"><strong>${c.portions} × ${esc(c.plat ?? 'Plat supprimé')}</strong>${badgeStatut(c.statut)}<span class="muted small">${esc(c.id)}</span></div>
            <span class="muted small">Pour ${esc(c.client)} · ${date(c.date, true)}</span>
            <span class="small">Livrer à ${esc(c.adresse || c.arrive)}${c.duree ? ` · environ ${minutes(c.duree)} de trajet` : ''}</span>
          </div>
          <div class="pile" style="gap:8px;align-items:end">
            <span class="prix">${prix(c.prix)}</span>
            <div class="rangee" style="gap:6px">
              <button class="btn btn-petit" data-trajet="${esc(c.id)}">Itinéraire</button>
              ${(ACTIONS[c.statut] || []).map(([statut, libelle, classe]) =>
                `<button class="btn btn-petit ${classe}" data-statut="${esc(statut)}" data-id="${esc(c.id)}">${libelle}</button>`).join('')}
            </div>
          </div>
        </div>`).join('')}</div>`;
    },

    async plats() {
      const plats = platsCache = await api('/api/cuisinier/plats');
      if (!plats.length) return vide('Aucun plat', 'Ajoutez votre premier plat pour apparaître dans le catalogue.');
      return `<div class="carte table-wrap"><table>
        <thead><tr><th></th><th>Plat</th><th>Cuisine</th><th class="nombre">Prix</th><th class="nombre">Stock</th><th class="nombre">Vendus</th><th>Périme le</th><th></th></tr></thead>
        <tbody>${plats.map(p => `
          <tr>
            <td><div class="mini-visuel" style="background:${visuelPlat(p)}">${esc(p.nom[0] ?? '?')}</div></td>
            <td><strong>${esc(p.nom)}</strong><div class="muted small">${esc(p.id)} · ${esc(p.ingredient)}</div></td>
            <td>${esc(p.nationalite)}${p.regime && p.regime !== 'Aucun' ? ` <span class="badge badge-ok">${esc(p.regime)}</span>` : ''}</td>
            <td class="nombre">${prix(p.prix)}</td>
            <td class="nombre">${p.disponible ? p.stock : `<span class="badge badge-danger">${p.stock ? 'Périmé' : 'Épuisé'}</span>`}</td>
            <td class="nombre">${p.vendus}</td>
            <td>${date(p.datePeremption)}</td>
            <td><div class="rangee" style="gap:4px;flex-wrap:nowrap"><button class="btn btn-petit" data-modifier="${esc(p.id)}">Modifier</button><button class="btn btn-petit btn-fantome btn-danger" data-supprimer="${esc(p.id)}" aria-label="Supprimer">✕</button></div></td>
          </tr>`).join('')}</tbody></table></div>`;
    },

    async clients() {
      const t = await api('/api/cuisinier/tableau');
      return `<div class="grille-2">
        <div class="carte"><div class="carte-corps"><h3 style="margin-bottom:12px">Clients servis</h3>
          ${t.clientsServis.length ? `<table><thead><tr><th>Client</th><th class="nombre">Commandes</th><th>Dernière</th></tr></thead><tbody>
            ${t.clientsServis.map(c => `<tr><td>${esc(c.nom)} <span class="muted small">${esc(c.id)}</span></td><td class="nombre">${c.commandes}</td><td>${date(c.derniere)}</td></tr>`).join('')}
          </tbody></table>` : '<p class="muted">Aucun client pour le moment.</p>'}</div></div>
        <div class="carte"><div class="carte-corps"><h3 style="margin-bottom:12px">Plats les plus commandés</h3>
          ${t.platsPopulaires.length ? `<table><thead><tr><th>Plat</th><th class="nombre">Portions</th></tr></thead><tbody>
            ${t.platsPopulaires.map(p => `<tr><td>${esc(p.nom)}</td><td class="nombre">${p.portions}</td></tr>`).join('')}
          </tbody></table>` : '<p class="muted">Aucune vente pour le moment.</p>'}</div></div>
      </div>`;
    }
  };

  let platsCache = [];
  const afficher = async () => {
    conteneur.querySelectorAll('[data-onglet]').forEach(b => b.classList.toggle('actif', b.dataset.onglet === onglet));
    contenu.innerHTML = '<div class="chargement">Chargement…</div>';
    contenu.innerHTML = await vues[onglet]();
    await kpis();
  };

  conteneur.querySelectorAll('[data-onglet]').forEach(b => b.onclick = () => { onglet = b.dataset.onglet; afficher(); });
  conteneur.querySelector('#nouveau-plat').onclick = () => formulairePlat(null, afficher);

  contenu.onclick = async e => {
    const cible = e.target.closest('button');
    if (!cible) return;
    try {
      if (cible.dataset.trajet) afficherTrajet(cible.dataset.trajet);
      else if (cible.dataset.statut) {
        await api(`/api/cuisinier/commandes/${encodeURIComponent(cible.dataset.id)}/statut`, { corps: { statut: cible.dataset.statut } });
        toast(`Commande ${cible.dataset.id} : ${cible.dataset.statut.toLowerCase()}`);
        await afficher();
      } else if (cible.dataset.modifier) {
        formulairePlat(platsCache.find(p => p.id === cible.dataset.modifier), afficher);
      } else if (cible.dataset.supprimer && confirm('Supprimer ce plat ?')) {
        await api(`/api/cuisinier/plats/${encodeURIComponent(cible.dataset.supprimer)}`, { method: 'DELETE' });
        toast('Plat supprimé.');
        await afficher();
      }
    } catch (err) { toast(err.message, true); }
  };

  await afficher();
  const minuteur = setInterval(() => {
    if (onglet === 'commandes' && !document.hidden && !document.getElementById('modale').open) afficher().catch(() => {});
  }, 20000);
  return () => clearInterval(minuteur);
}

function formulairePlat(plat, apres) {
  const jours = plat ? Math.max(0, Math.round((new Date(plat.datePeremption) - new Date(plat.dateFabrication)) / 86400000)) : 3;
  const corps = etat.ouvrirModale(plat ? `Modifier ${plat.nom}` : 'Nouveau plat', `
    <form class="pile" id="form-plat">
      <div class="grille-champs">
        <div class="champ pleine"><label>Nom du plat</label><input name="nom" required maxlength="50" value="${esc(plat?.nom)}" placeholder="Ex : Lasagnes maison"></div>
        <div class="champ pleine"><label>Description</label><textarea name="description" rows="2" maxlength="255" placeholder="Ce qui rend votre plat unique">${esc(plat?.description)}</textarea></div>
        <div class="champ"><label>Cuisine</label><input name="nationalite" required maxlength="50" value="${esc(plat?.nationalite)}" placeholder="Ex : Italienne" list="liste-nationalites"></div>
        <div class="champ"><label>Régime</label><input name="regime" maxlength="50" value="${esc(plat?.regime ?? 'Aucun')}" list="liste-regimes"></div>
        <div class="champ"><label>Ingrédient principal</label><input name="ingredient" required maxlength="50" value="${esc(plat?.ingredient)}"></div>
        <div class="champ"><label>Prix par portion (€)</label><input name="prix" type="number" step="0.5" min="0.5" max="500" required value="${plat?.prix ?? ''}"></div>
        <div class="champ"><label>Portions disponibles</label><input name="stock" type="number" min="0" max="1000" required value="${plat?.stock ?? 10}"></div>
        <div class="champ"><label>Se conserve (jours)</label><input name="jours" type="number" min="0" max="365" required value="${jours}"></div>
      </div>
      <datalist id="liste-nationalites"><option>Française</option><option>Italienne</option><option>Japonaise</option><option>Maghrébine</option><option>Espagnole</option><option>Américaine</option><option>Indienne</option><option>Libanaise</option></datalist>
      <datalist id="liste-regimes"><option>Aucun</option><option>Végétarien</option><option>Végan</option><option>Halal</option><option>Sans gluten</option><option>Poisson</option></datalist>
      <button class="btn btn-primaire btn-large">${plat ? 'Enregistrer' : 'Mettre en vente'}</button>
    </form>`);
  corps.querySelector('#form-plat').onsubmit = async ev => {
    ev.preventDefault();
    const f = ev.target.elements;
    const donnees = {
      nom: f.nom.value, description: f.description.value, nationalite: f.nationalite.value, regime: f.regime.value,
      ingredient: f.ingredient.value, prix: Number(f.prix.value), stock: Number(f.stock.value), joursConservation: Number(f.jours.value)
    };
    try {
      if (plat) await api(`/api/cuisinier/plats/${encodeURIComponent(plat.id)}`, { method: 'PUT', corps: donnees });
      else await api('/api/cuisinier/plats', { corps: donnees });
      etat.fermerModale();
      toast(plat ? 'Plat mis à jour.' : 'Plat mis en vente !');
      apres();
    } catch (err) { toast(err.message, true); }
  };
}
