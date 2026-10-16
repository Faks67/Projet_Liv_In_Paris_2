// Administration : statistiques, clients, cuisiniers, commandes, coloration, exports
import { api, esc, prix, date, badgeStatut, vide, toast } from '../outils.js';
import { afficherTrajet } from './commandes.js';

const PALETTE = ['#d9480f', '#1c6dd0', '#2b8a3e', '#b7791f', '#7048e8', '#0c8599', '#c2255c', '#5c940d', '#495057', '#e8590c'];

export async function vueAdmin(conteneur, [ongletInitial]) {
  let onglet = ongletInitial || 'stats';
  let graphiques = [];
  conteneur.innerHTML = `
    <div class="titre-section"><div><h1>Administration</h1><p class="muted">Pilotage de la plateforme.</p></div></div>
    <div class="onglets">
      <button data-onglet="stats">Vue d'ensemble</button><button data-onglet="commandes">Commandes</button>
      <button data-onglet="clients">Clients</button><button data-onglet="cuisiniers">Cuisiniers</button>
      <button data-onglet="coloration">Coloration</button><button data-onglet="export">Exports</button>
    </div>
    <div id="contenu"></div>`;
  const contenu = conteneur.querySelector('#contenu');

  const detruireGraphiques = () => { graphiques.forEach(g => g.destroy()); graphiques = []; };
  const couleurTexte = () => getComputedStyle(document.documentElement).getPropertyValue('--text-2').trim();

  const vues = {
    async stats() {
      const s = await api('/api/admin/stats');
      contenu.innerHTML = `
        <div class="kpis">
          <div class="carte kpi"><span class="muted small">Chiffre d'affaires</span><strong>${prix(s.chiffreAffaires)}</strong></div>
          <div class="carte kpi"><span class="muted small">Commandes</span><strong>${s.commandes}</strong></div>
          <div class="carte kpi"><span class="muted small">Panier moyen</span><strong>${prix(s.panierMoyen)}</strong></div>
          <div class="carte kpi"><span class="muted small">Dépense moyenne / client</span><strong>${prix(s.depenseMoyenneClient)}</strong></div>
          <div class="carte kpi"><span class="muted small">Clients · Cuisiniers · Plats</span><strong>${s.clients} · ${s.cuisiniers} · ${s.plats}</strong></div>
        </div>
        <div class="grille-2">
          <div class="carte"><div class="carte-corps"><h3>Commandes des 30 derniers jours</h3><canvas id="g-jours" height="220"></canvas></div></div>
          <div class="carte"><div class="carte-corps"><h3>Livraisons par cuisinier</h3><canvas id="g-cuisiniers" height="220"></canvas></div></div>
          <div class="carte"><div class="carte-corps"><h3>Commandes par cuisine</h3><canvas id="g-nationalites" height="220"></canvas></div></div>
          <div class="carte"><div class="carte-corps"><h3>Meilleurs clients</h3>
            <table><thead><tr><th>Client</th><th class="nombre">Commandes</th><th class="nombre">Total</th></tr></thead><tbody>
              ${s.topClients.map(c => `<tr><td>${esc(c.client)} <span class="muted small">${esc(c.id)}</span></td><td class="nombre">${c.commandes}</td><td class="nombre">${prix(c.total)}</td></tr>`).join('')}
            </tbody></table></div></div>
        </div>`;
      Chart.defaults.color = couleurTexte();
      Chart.defaults.font.family = 'Inter, system-ui, sans-serif';
      const options = { plugins: { legend: { display: false } }, scales: { y: { beginAtZero: true, ticks: { precision: 0 } } } };
      graphiques.push(new Chart(contenu.querySelector('#g-jours'), {
        type: 'bar',
        data: { labels: s.parJour.map(j => date(j.jour)), datasets: [{ data: s.parJour.map(j => j.commandes), backgroundColor: PALETTE[0], borderRadius: 6, maxBarThickness: 36 }] },
        options
      }));
      graphiques.push(new Chart(contenu.querySelector('#g-cuisiniers'), {
        type: 'bar',
        data: { labels: s.livraisonsParCuisinier.map(c => c.cuisinier), datasets: [{ data: s.livraisonsParCuisinier.map(c => c.livraisons), backgroundColor: PALETTE[1], borderRadius: 6, maxBarThickness: 28 }] },
        options: { ...options, indexAxis: 'y', scales: { x: { beginAtZero: true, ticks: { precision: 0 } } } }
      }));
      graphiques.push(new Chart(contenu.querySelector('#g-nationalites'), {
        type: 'doughnut',
        data: { labels: s.parNationalite.map(n => n.nationalite), datasets: [{ data: s.parNationalite.map(n => n.commandes), backgroundColor: PALETTE, borderWidth: 0 }] },
        options: { plugins: { legend: { position: 'right' } }, cutout: '62%' }
      }));
    },

    async commandes() {
      contenu.innerHTML = `
        <div class="carte" style="margin-bottom:16px"><div class="carte-corps">
          <form class="rangee" id="filtres">
            <div class="champ"><label>Du</label><input type="date" name="debut"></div>
            <div class="champ"><label>Au</label><input type="date" name="fin"></div>
            <div class="champ"><label>Client (ID)</label><input name="client" placeholder="CL1"></div>
            <button class="btn btn-primaire" style="align-self:end">Filtrer</button>
          </form>
        </div></div>
        <div id="liste"></div>`;
      const charger = async () => {
        const f = contenu.querySelector('#filtres').elements;
        const params = new URLSearchParams();
        if (f.debut.value) params.set('debut', f.debut.value);
        if (f.fin.value) params.set('fin', f.fin.value);
        if (f.client.value.trim()) params.set('client', f.client.value.trim());
        const commandes = await api(`/api/admin/commandes?${params}`);
        contenu.querySelector('#liste').innerHTML = commandes.length ? `
          <div class="carte table-wrap"><table>
            <thead><tr><th>Commande</th><th>Date</th><th>Plat</th><th>Client</th><th>Cuisinier</th><th>Trajet</th><th>Statut</th><th class="nombre">Prix</th><th></th></tr></thead>
            <tbody>${commandes.map(c => `<tr>
              <td>${esc(c.id)}</td><td>${date(c.date, true)}</td><td>${c.portions} × ${esc(c.plat ?? '—')}</td>
              <td>${esc(c.client)}</td><td>${esc(c.cuisinier)}</td><td class="small">${esc(c.depart)} → ${esc(c.arrive)}</td>
              <td>${badgeStatut(c.statut)}</td><td class="nombre">${prix(c.prix)}</td>
              <td><button class="btn btn-petit" data-trajet="${esc(c.id)}">Trajet</button></td></tr>`).join('')}</tbody>
          </table></div>` : vide('Aucune commande', 'Aucune commande ne correspond à ces filtres.');
      };
      contenu.querySelector('#filtres').onsubmit = e => { e.preventDefault(); charger(); };
      await charger();
    },

    async clients() {
      const clients = await api('/api/admin/clients');
      contenu.innerHTML = `<div class="carte table-wrap"><table>
        <thead><tr><th>ID</th><th>Nom</th><th>Adresse</th><th>Station</th><th class="nombre">Commandes</th><th class="nombre">Total</th><th></th></tr></thead>
        <tbody>${clients.map(c => `<tr>
          <td>${esc(c.id)}</td><td><strong>${esc(c.prenom)} ${esc(c.nom)}</strong><div class="muted small">${esc(c.utilisateur)}</div></td>
          <td class="small">${esc(c.adresse)}</td><td>${esc(c.station)}</td>
          <td class="nombre">${c.commandes}</td><td class="nombre">${prix(c.total)}</td>
          <td><button class="btn btn-petit btn-fantome btn-danger" data-supprimer-client="${esc(c.id)}">Supprimer</button></td></tr>`).join('')}</tbody>
      </table></div>`;
    },

    async cuisiniers() {
      const cuisiniers = await api('/api/admin/cuisiniers');
      contenu.innerHTML = `<div class="carte table-wrap"><table>
        <thead><tr><th>ID</th><th>Nom</th><th>Contact</th><th>Station</th><th class="nombre">Plats</th><th class="nombre">Livraisons</th><th></th></tr></thead>
        <tbody>${cuisiniers.map(c => `<tr>
          <td>${esc(c.id)}</td><td><strong>${esc(c.prenom)} ${esc(c.nom)}</strong><div class="muted small">${esc(c.adresse)}</div></td>
          <td class="small">${esc(c.email)}<br>${esc(c.telephone)}</td><td>${esc(c.station)}</td>
          <td class="nombre">${c.plats}</td><td class="nombre">${c.livraisons}</td>
          <td><button class="btn btn-petit btn-fantome btn-danger" data-supprimer-cuisinier="${esc(c.id)}">Supprimer</button></td></tr>`).join('')}</tbody>
      </table></div>`;
    },

    async coloration() {
      const g = await api('/api/admin/coloration');
      if (!g.sommets.length) { contenu.innerHTML = vide('Pas assez de données', 'Il faut des commandes pour relier les cuisiniers.'); return; }
      const taille = 520, centre = taille / 2, rayon = 190;
      const pos = Object.fromEntries(g.sommets.map((s, i) => {
        const a = (2 * Math.PI * i) / g.sommets.length - Math.PI / 2;
        return [s.id, [centre + rayon * Math.cos(a), centre + rayon * Math.sin(a)]];
      }));
      contenu.innerHTML = `
        <div class="grille-2">
          <div class="carte"><div class="carte-corps">
            <svg viewBox="0 0 ${taille} ${taille}" style="width:100%;height:auto" role="img" aria-label="Graphe des cuisiniers colorié">
              ${g.aretes.map(a => `<line x1="${pos[a.de][0]}" y1="${pos[a.de][1]}" x2="${pos[a.vers][0]}" y2="${pos[a.vers][1]}" stroke="var(--border)" stroke-width="3"/>`).join('')}
              ${g.sommets.map(s => `
                <g><circle cx="${pos[s.id][0]}" cy="${pos[s.id][1]}" r="26" fill="${PALETTE[s.couleur % PALETTE.length]}" stroke="var(--surface)" stroke-width="4"/>
                <text x="${pos[s.id][0]}" y="${pos[s.id][1] + 5}" text-anchor="middle" fill="#fff" font-weight="800" font-size="14">${esc(s.id)}</text>
                <text x="${pos[s.id][0]}" y="${pos[s.id][1] + 46}" text-anchor="middle" fill="var(--text)" font-size="13">${esc(s.nom)}</text></g>`).join('')}
            </svg>
          </div></div>
          <div class="carte"><div class="carte-corps pile">
            <h3>Coloration de Welsh-Powell</h3>
            <p class="muted">Deux cuisiniers sont reliés lorsqu'ils ont servi au moins un client commun. L'algorithme attribue une couleur à chaque cuisinier pour que deux cuisiniers reliés n'aient jamais la même.</p>
            <div class="kpi" style="padding:0"><span class="muted small">Nombre de couleurs utilisées</span><strong>${g.nombreCouleurs}</strong></div>
            <table><thead><tr><th>Cuisinier</th><th>Couleur</th><th class="nombre">Clients</th></tr></thead><tbody>
              ${g.sommets.map(s => `<tr><td>${esc(s.nom)}</td><td><span class="badge" style="background:${PALETTE[s.couleur % PALETTE.length]};color:#fff">Couleur ${s.couleur + 1}</span></td><td class="nombre">${s.clients}</td></tr>`).join('')}
            </tbody></table>
          </div></div>
        </div>`;
    },

    async export() {
      contenu.innerHTML = `<div class="carte"><div class="carte-corps pile">
        <h3>Exporter les données</h3><p class="muted">Téléchargez chaque table au format JSON.</p>
        <div class="rangee">${['Client', 'Cuisinier', 'Plat', 'Commande'].map(t => `<a class="btn" href="/api/admin/export/${t}" download>${t}s (.json)</a>`).join('')}</div>
      </div></div>`;
    }
  };

  const afficher = async () => {
    detruireGraphiques();
    conteneur.querySelectorAll('[data-onglet]').forEach(b => b.classList.toggle('actif', b.dataset.onglet === onglet));
    contenu.innerHTML = '<div class="chargement">Chargement…</div>';
    await vues[onglet]();
  };

  conteneur.querySelectorAll('[data-onglet]').forEach(b => b.onclick = () => { onglet = b.dataset.onglet; afficher(); });
  contenu.onclick = async e => {
    const b = e.target.closest('button');
    if (!b) return;
    try {
      if (b.dataset.trajet) afficherTrajet(b.dataset.trajet);
      if (b.dataset.supprimerClient && confirm(`Supprimer le client ${b.dataset.supprimerClient} et toutes ses commandes ?`)) {
        await api(`/api/admin/clients/${encodeURIComponent(b.dataset.supprimerClient)}`, { method: 'DELETE' });
        toast('Client supprimé.');
        await afficher();
      }
      if (b.dataset.supprimerCuisinier && confirm(`Supprimer le cuisinier ${b.dataset.supprimerCuisinier}, ses plats et ses commandes ?`)) {
        await api(`/api/admin/cuisiniers/${encodeURIComponent(b.dataset.supprimerCuisinier)}`, { method: 'DELETE' });
        toast('Cuisinier supprimé.');
        await afficher();
      }
    } catch (err) { toast(err.message, true); }
  };

  await afficher();
  return detruireGraphiques;
}
