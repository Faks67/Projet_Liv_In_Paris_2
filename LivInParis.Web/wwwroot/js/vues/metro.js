// Plan du métro et calcul d'itinéraire entre deux adresses ou stations (Dijkstra côté serveur)
import { api, esc, toast } from '../outils.js';
import { champAdresse } from '../adresse.js';
import { creerCarte, dessinerReseau, dessinerTrajet, feuilleDeRoute } from '../carte.js';

export async function vueMetro(conteneur) {
  conteneur.innerHTML = `
    <div class="titre-section"><div><h1>Plan du métro</h1><p class="muted">Le trajet le plus rapide entre deux adresses ou stations, correspondances comprises.</p></div></div>
    <div class="validation">
      <div class="pile">
        <div class="carte"><div class="carte-corps pile">
          <div class="champ"><label for="depart">Départ</label><input id="depart" placeholder="Adresse ou station"></div>
          <div class="rangee"><button class="btn btn-petit btn-fantome" id="inverser">⇅ Inverser</button></div>
          <div class="champ"><label for="arrivee">Arrivée</label><input id="arrivee" placeholder="Adresse ou station"></div>
          <p class="muted small">Astuce : cliquez sur deux stations de la carte.</p>
        </div></div>
        <div class="carte hidden" id="bloc-route"><div class="carte-corps" id="route"></div></div>
      </div>
      <div class="carte-map haute" id="carte"></div>
    </div>`;

  const carte = creerCarte(conteneur.querySelector('#carte'));
  let depart = null, arrivee = null, calque = null;
  const champs = { depart: conteneur.querySelector('#depart'), arrivee: conteneur.querySelector('#arrivee') };

  const calculer = async () => {
    if (!depart || !arrivee) return;
    const params = new URLSearchParams({ deLat: depart.lat, deLon: depart.lon, deNom: depart.libelle, versLat: arrivee.lat, versLon: arrivee.lon, versNom: arrivee.libelle });
    try {
      const trajet = await api(`/api/metro/trajet?${params}`);
      calque = dessinerTrajet(carte, trajet, calque);
      conteneur.querySelector('#bloc-route').classList.remove('hidden');
      conteneur.querySelector('#route').innerHTML = feuilleDeRoute(trajet);
    } catch (err) { toast(err.message, true); }
  };

  champAdresse(champs.depart, p => { depart = p; calculer(); }, { stations: true });
  champAdresse(champs.arrivee, p => { arrivee = p; calculer(); }, { stations: true });

  conteneur.querySelector('#inverser').onclick = () => {
    [depart, arrivee] = [arrivee, depart];
    [champs.depart.value, champs.arrivee.value] = [champs.arrivee.value, champs.depart.value];
    calculer();
  };

  await dessinerReseau(carte, {
    stationsCliquables: s => {
      const point = { libelle: s.nom, lat: s.lat, lon: s.lon };
      if (!depart || (depart && arrivee)) {
        depart = point; arrivee = null;
        champs.depart.value = s.nom; champs.arrivee.value = '';
        toast(`Départ : ${s.nom}. Choisissez l'arrivée.`);
      } else {
        arrivee = point;
        champs.arrivee.value = s.nom;
        calculer();
      }
    }
  });
  return () => carte.remove();
}
