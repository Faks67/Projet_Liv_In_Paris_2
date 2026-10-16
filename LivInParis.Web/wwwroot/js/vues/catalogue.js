// Catalogue des plats : recherche, filtres, ajout au panier
import { api, esc, prix, date, visuelPlat, vide, toast } from '../outils.js';
import * as etat from '../etat.js';

export async function vueCatalogue(conteneur) {
  const [plats, filtres] = await Promise.all([api('/api/plats'), api('/api/plats/filtres')]);
  let recherche = '', nationalite = '', regime = '';
  const cuisiniers = new Set(plats.map(p => p.idCuisinier)).size;
  const disponibles = plats.filter(p => p.disponible).length;

  conteneur.innerHTML = `
    <section class="hero">
      <div>
        <h1>Le fait maison de vos voisins, livré en métro.</h1>
        <p>Des cuisiniers parisiens préparent leurs plats chez eux et vous les livrent par le métro. Entrez votre adresse, on calcule le trajet le plus rapide.</p>
        <div class="rangee" style="margin-top:20px">
          <a class="btn btn-primaire" href="#plats">Voir les plats</a>
          <a class="btn" href="#/metro">Calculer un trajet</a>
        </div>
      </div>
      <div class="hero-stats">
        <div class="hero-stat"><strong>${disponibles}</strong><span class="muted small">plats disponibles</span></div>
        <div class="hero-stat"><strong>${cuisiniers}</strong><span class="muted small">cuisiniers</span></div>
        <div class="hero-stat"><strong>16</strong><span class="muted small">lignes de métro</span></div>
      </div>
    </section>

    <div class="titre-section" id="plats"><h2>Les plats du moment</h2></div>
    <div class="filtres">
      <input class="recherche" id="recherche" type="search" placeholder="Rechercher un plat, un ingrédient…">
      <select id="filtre-regime" style="width:auto">
        <option value="">Tous les régimes</option>
        ${filtres.regimes.map(r => `<option>${esc(r)}</option>`).join('')}
      </select>
    </div>
    <div class="puces" id="puces" style="margin-bottom:20px">
      <button class="puce active" data-nat="">Toutes les cuisines</button>
      ${filtres.nationalites.map(n => `<button class="puce" data-nat="${esc(n)}">${esc(n)}</button>`).join('')}
    </div>
    <div id="grille"></div>`;

  const grille = conteneur.querySelector('#grille');
  const dessiner = () => {
    const q = recherche.toLowerCase();
    const liste = plats.filter(p =>
      (!nationalite || p.nationalite === nationalite) && (!regime || p.regime === regime) &&
      (!q || [p.nom, p.ingredient, p.nationalite, p.description, p.cuisinier].some(v => (v || '').toLowerCase().includes(q))));
    grille.innerHTML = liste.length
      ? `<div class="grille-plats">${liste.map(carte).join('')}</div>`
      : vide('Aucun plat trouvé', 'Essayez une autre recherche ou un autre filtre.');
  };

  conteneur.querySelector('#recherche').oninput = e => { recherche = e.target.value; dessiner(); };
  conteneur.querySelector('#filtre-regime').onchange = e => { regime = e.target.value; dessiner(); };
  conteneur.querySelector('#puces').onclick = e => {
    const puce = e.target.closest('[data-nat]');
    if (!puce) return;
    nationalite = puce.dataset.nat;
    conteneur.querySelectorAll('#puces .puce').forEach(b => b.classList.toggle('active', b === puce));
    dessiner();
  };
  conteneur.querySelector('a[href="#plats"]').onclick = e => {
    e.preventDefault();
    conteneur.querySelector('#plats').scrollIntoView({ behavior: 'smooth' });
  };
  grille.onclick = e => {
    const ajout = e.target.closest('[data-ajouter]');
    const fiche = e.target.closest('[data-plat]');
    if (ajout) { e.stopPropagation(); ajouter(plats.find(p => p.id === ajout.dataset.ajouter)); }
    else if (fiche) detail(plats.find(p => p.id === fiche.dataset.plat));
  };
  dessiner();
}

function carte(p) {
  const stock = !p.disponible ? '<span class="badge badge-blanc">Épuisé</span>'
    : p.stock <= 5 ? `<span class="badge badge-blanc">Plus que ${p.stock}</span>` : '';
  return `
    <article class="carte plat ${p.disponible ? '' : 'indisponible'}" data-plat="${esc(p.id)}" style="cursor:pointer">
      <div class="plat-visuel" style="background:${visuelPlat(p)}">${stock}${esc(p.nom[0] ?? '?')}</div>
      <div class="plat-corps">
        <div class="rangee" style="gap:6px"><span class="badge">${esc(p.nationalite)}</span>${p.regime && p.regime !== 'Aucun' ? `<span class="badge badge-ok">${esc(p.regime)}</span>` : ''}</div>
        <h3>${esc(p.nom)}</h3>
        <p class="muted small">${esc(p.description || `À base de ${(p.ingredient || '').toLowerCase()}`)}</p>
        <p class="small">Par <strong>${esc(p.cuisinier)}</strong> · Ⓜ ${esc(p.station)}</p>
        <div class="plat-pied">
          <span class="prix">${prix(p.prix)}</span>
          <button class="btn btn-primaire btn-petit" data-ajouter="${esc(p.id)}" ${p.disponible ? '' : 'disabled'}>Ajouter</button>
        </div>
      </div>
    </article>`;
}

function ajouter(plat) {
  if (!plat?.disponible) return;
  if (etat.ajouterAuPanier(plat)) toast(`${plat.nom} ajouté au panier`);
  else toast(`Stock maximum atteint pour ${plat.nom}.`, true);
}

function detail(p) {
  const corps = etat.ouvrirModale(p.nom, `
    <div class="pile">
      <div class="plat-visuel" style="background:${visuelPlat(p)};border-radius:12px">${esc(p.nom[0] ?? '?')}</div>
      <p>${esc(p.description || `Un plat ${(p.nationalite || '').toLowerCase()} à base de ${(p.ingredient || '').toLowerCase()}.`)}</p>
      <table>
        <tr><th>Cuisinier</th><td>${esc(p.cuisinier)}</td></tr>
        <tr><th>Part de</th><td>Ⓜ ${esc(p.station)}</td></tr>
        <tr><th>Cuisine</th><td>${esc(p.nationalite)}</td></tr>
        <tr><th>Régime</th><td>${esc(p.regime)}</td></tr>
        <tr><th>Ingrédient principal</th><td>${esc(p.ingredient)}</td></tr>
        <tr><th>Préparé le</th><td>${date(p.dateFabrication)}</td></tr>
        <tr><th>À consommer avant</th><td>${date(p.datePeremption)}</td></tr>
        <tr><th>Portions restantes</th><td>${p.stock}</td></tr>
      </table>
      <div class="rangee"><span class="prix">${prix(p.prix)}</span><span class="espace"></span>
        <button class="btn btn-primaire" id="ajout-detail" ${p.disponible ? '' : 'disabled'}>${p.disponible ? 'Ajouter au panier' : 'Indisponible'}</button></div>
    </div>`);
  corps.querySelector('#ajout-detail').onclick = () => { ajouter(p); etat.fermerModale(); };
}
