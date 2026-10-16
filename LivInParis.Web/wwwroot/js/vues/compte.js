// Connexion, inscription, profil
import { api, esc, toast } from '../outils.js';
import * as etat from '../etat.js';
import { champAdresse } from '../adresse.js';

export function menuCompte() {
  const s = etat.session;
  if (!s.connecte) return '<button class="btn btn-primaire" data-action="connexion">Se connecter</button>';
  const nom = s.admin ? 'Admin' : (s.client ?? s.cuisinier)?.prenom ?? s.idUtilisateur;
  return `<a class="btn btn-fantome" href="#/profil" title="Mon profil">
    <span class="logo-pastille" style="width:28px;height:28px;font-size:.8rem;border-radius:50%">${esc(nom[0]?.toUpperCase())}</span>
    <span class="small">${esc(nom)}</span></a>`;
}

document.addEventListener('click', e => {
  if (e.target.closest('[data-action="connexion"]')) demanderConnexion();
});

const signaler = () => window.dispatchEvent(new Event('livinparis:session'));

/**
 * Ouvre la fenêtre de connexion / inscription. "apres" est appelé une fois connecté.
 */
export function demanderConnexion(apres, ongletInitial = 'connexion') {
  const corps = etat.ouvrirModale('Bienvenue', `
    <div class="onglets"><button data-onglet="connexion">Connexion</button><button data-onglet="inscription">Créer un compte</button></div>
    <div id="onglet-contenu"></div>`);

  const afficher = onglet => {
    corps.querySelectorAll('[data-onglet]').forEach(b => b.classList.toggle('actif', b.dataset.onglet === onglet));
    const zone = corps.querySelector('#onglet-contenu');
    if (onglet === 'connexion') {
      zone.innerHTML = `
        <form class="pile" id="form-connexion">
          <div class="champ"><label for="c-id">Identifiant</label><input id="c-id" required autocomplete="username"></div>
          <div class="champ"><label for="c-mdp">Mot de passe</label><input id="c-mdp" type="password" required autocomplete="current-password"></div>
          <button class="btn btn-primaire btn-large">Se connecter</button>
          <p class="muted small">Comptes de démonstration : U1 à U5 (mots de passe pass123, pass234…), admin / admin.</p>
        </form>`;
      zone.querySelector('#form-connexion').onsubmit = async ev => {
        ev.preventDefault();
        try {
          await api('/api/connexion', { corps: { identifiant: zone.querySelector('#c-id').value, motDePasse: zone.querySelector('#c-mdp').value } });
          await terminer();
        } catch (err) { toast(err.message, true); }
      };
      zone.querySelector('#c-id').focus();
    } else {
      zone.innerHTML = formulaireInscription(true);
      brancherInscription(zone, async () => terminer());
    }
  };

  const terminer = async () => {
    await etat.rafraichirSession();
    etat.fermerModale();
    toast('Vous êtes connecté.');
    signaler();
    apres?.();
  };

  corps.querySelectorAll('[data-onglet]').forEach(b => b.onclick = () => afficher(b.dataset.onglet));
  afficher(ongletInitial);
}

function formulaireInscription(avecIdentifiants, role = 'client') {
  return `
    <form class="pile" id="form-inscription">
      <div class="champ"><label>Je veux</label>
        <div class="puces">
          <button type="button" class="puce ${role === 'client' ? 'active' : ''}" data-role="client">Commander des plats</button>
          <button type="button" class="puce ${role === 'cuisinier' ? 'active' : ''}" data-role="cuisinier">Cuisiner et vendre</button>
        </div>
      </div>
      <div class="grille-champs">
        ${avecIdentifiants ? `
        <div class="champ"><label for="i-id">Identifiant</label><input id="i-id" required maxlength="50" autocomplete="username"></div>
        <div class="champ"><label for="i-mdp">Mot de passe</label><input id="i-mdp" type="password" required minlength="6" autocomplete="new-password"></div>` : ''}
        <div class="champ"><label for="i-prenom">Prénom</label><input id="i-prenom" required maxlength="50"></div>
        <div class="champ"><label for="i-nom">Nom</label><input id="i-nom" required maxlength="50"></div>
        <div class="champ pleine"><label for="i-adresse">Adresse</label><input id="i-adresse" required placeholder="Ex : 10 rue de Rivoli, Paris"></div>
        <div class="champ pleine muted small" id="i-station"></div>
        <div class="champ cuisinier-seul"><label for="i-email">E-mail</label><input id="i-email" type="email" maxlength="50"></div>
        <div class="champ cuisinier-seul"><label for="i-tel">Téléphone</label><input id="i-tel" maxlength="20"></div>
      </div>
      <button class="btn btn-primaire btn-large">Créer mon compte</button>
    </form>`;
}

function brancherInscription(zone, apres) {
  let role = 'client';
  let adresse = null;
  const majRole = () => {
    zone.querySelectorAll('[data-role]').forEach(b => b.classList.toggle('active', b.dataset.role === role));
    zone.querySelectorAll('.cuisinier-seul').forEach(el => el.classList.toggle('hidden', role !== 'cuisinier'));
  };
  zone.querySelectorAll('[data-role]').forEach(b => b.onclick = () => { role = b.dataset.role; majRole(); });
  majRole();
  champAdresse(zone.querySelector('#i-adresse'), async a => {
    adresse = a;
    const { station, minutesAPied } = await api(`/api/metro/proche?lat=${a.lat}&lon=${a.lon}`);
    zone.querySelector('#i-station').textContent = `Station la plus proche : ${station.nom} (${minutesAPied} min à pied)`;
  });
  zone.querySelector('#form-inscription').onsubmit = async ev => {
    ev.preventDefault();
    if (!adresse || !zone.querySelector('#i-adresse').dataset.valide) { toast("Choisissez l'adresse dans la liste proposée.", true); return; }
    const val = id => zone.querySelector(id)?.value ?? null;
    const codePostal = (adresse.libelle.match(/\b(75|77|78|91|92|93|94|95)\d{3}\b/) || [])[0];
    try {
      await api('/api/inscription', {
        corps: {
          role, identifiant: val('#i-id'), motDePasse: val('#i-mdp'), nom: val('#i-nom'), prenom: val('#i-prenom'),
          adresse: adresse.libelle, lat: adresse.lat, lon: adresse.lon, email: val('#i-email'), telephone: val('#i-tel'),
          codePostal: codePostal ? Number(codePostal) : null
        }
      });
      await apres();
    } catch (err) { toast(err.message, true); }
  };
}

// ---------- Page profil ----------
export async function vueProfil(conteneur) {
  const s = etat.session;
  const blocProfil = (role, p) => `
    <div class="carte"><div class="carte-corps pile">
      <div class="rangee"><h3>${role === 'client' ? 'Profil client' : 'Profil cuisinier'}</h3><span class="badge">${esc(p.id)}</span></div>
      <form class="pile" data-profil="${role}">
        <div class="grille-champs">
          <div class="champ"><label>Prénom</label><input name="prenom" value="${esc(p.prenom)}" required maxlength="50"></div>
          <div class="champ"><label>Nom</label><input name="nom" value="${esc(p.nom)}" required maxlength="50"></div>
          <div class="champ pleine"><label>Adresse</label><input name="adresse" value="${esc(p.adresse)}" required></div>
          <div class="champ pleine muted small" data-station>Station : ${esc(p.station)}</div>
          ${role === 'cuisinier' ? `
          <div class="champ"><label>E-mail</label><input name="email" type="email" value="${esc(p.email)}" maxlength="50"></div>
          <div class="champ"><label>Téléphone</label><input name="telephone" value="${esc(p.telephone)}" maxlength="20"></div>` : ''}
        </div>
        <div><button class="btn btn-primaire">Enregistrer</button></div>
      </form>
    </div></div>`;

  conteneur.innerHTML = `
    <div class="titre-section"><div><h1>Mon profil</h1><p class="muted">Connecté en tant que ${esc(s.idUtilisateur)}</p></div>
      <button class="btn" id="deconnexion">Se déconnecter</button></div>
    <div class="grille-2">
      ${s.client ? blocProfil('client', s.client) : ''}
      ${s.cuisinier ? blocProfil('cuisinier', s.cuisinier) : ''}
      ${s.admin ? '<div class="carte"><div class="carte-corps"><h3>Administrateur</h3><p class="muted">Le compte admin gère le site depuis la page Administration.</p></div></div>' : ''}
      ${!s.admin && (!s.client || !s.cuisinier) ? `
        <div class="carte"><div class="carte-corps pile">
          <h3>${s.client ? 'Devenir cuisinier' : 'Commander aussi des plats'}</h3>
          <p class="muted">${s.client ? 'Proposez vos plats aux Parisiens avec le même compte.' : 'Créez un profil client pour commander avec le même compte.'}</p>
          <div><button class="btn" id="ajouter-role">${s.client ? 'Créer mon profil cuisinier' : 'Créer mon profil client'}</button></div>
        </div></div>` : ''}
    </div>`;

  conteneur.querySelector('#deconnexion').onclick = async () => {
    await api('/api/deconnexion', { method: 'POST' });
    await etat.rafraichirSession();
    toast('À bientôt !');
    location.hash = '#/';
  };

  conteneur.querySelectorAll('[data-profil]').forEach(form => {
    const role = form.dataset.profil;
    let point = null;
    champAdresse(form.elements.adresse, async a => {
      point = a;
      const { station } = await api(`/api/metro/proche?lat=${a.lat}&lon=${a.lon}`);
      form.querySelector('[data-station]').textContent = `Station : ${station.nom}`;
    });
    form.onsubmit = async ev => {
      ev.preventDefault();
      const p = s[role];
      const corps = {
        nom: form.elements.nom.value, prenom: form.elements.prenom.value, adresse: form.elements.adresse.value,
        lat: point?.lat ?? null, lon: point?.lon ?? null, station: point ? null : p.station,
        email: form.elements.email?.value ?? null, telephone: form.elements.telephone?.value ?? null, codePostal: p.codePostal ?? null
      };
      try {
        await api(`/api/profil/${role}`, { method: 'PUT', corps });
        await etat.rafraichirSession();
        toast('Profil mis à jour.');
      } catch (err) { toast(err.message, true); }
    };
  });

  const ajouter = conteneur.querySelector('#ajouter-role');
  if (ajouter) ajouter.onclick = () => {
    const role = s.client ? 'cuisinier' : 'client';
    const corps = etat.ouvrirModale(role === 'cuisinier' ? 'Profil cuisinier' : 'Profil client', formulaireInscription(false, role));
    brancherInscription(corps, async () => {
      await etat.rafraichirSession();
      etat.fermerModale();
      toast('Profil créé.');
      signaler();
    });
    corps.querySelector(`[data-role="${role}"]`).click();
  };
}
