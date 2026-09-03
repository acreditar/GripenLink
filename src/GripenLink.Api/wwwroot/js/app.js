// GripenLink — mapa vivo + MDIs (valores puros do cockpit, sem IA slop)
const $ = s => document.getElementById(s);
const fmt = {
  f1: v => isFinite(v) ? v.toFixed(1) : "—",
  f2: v => isFinite(v) ? v.toFixed(2) : "—",
  i: v => isFinite(v) ? Math.round(v).toString() : "—",
  ft: m => isFinite(m) ? Math.round(m*3.28084).toString() : "—",
  kt: mps => isFinite(mps) ? (mps*1.94384).toFixed(0) : "—",
  coord: v => isFinite(v) ? v.toFixed(5) : "—",
};

// Leaflet — carto dark, militar
const map = L.map('map', {zoomControl:false, attributionControl:true}).setView([-23.18,-45.86], 11);
L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {attribution:'© OSM • GripenLink', maxZoom:18}).addTo(map);
L.control.zoom({position:'topright'}).addTo(map);

let trail = [];
let poly = L.polyline([], {color:'#33ff66', weight:2, opacity:.9}).addTo(map);
let marker = null;

function acIcon(hdg){
  const el = document.createElement('div'); el.className='ac-marker';
  el.style.transform = `rotate(${hdg}deg)`;
  return L.divIcon({className:'', html: el, iconSize:[14,14], iconAnchor:[7,7]});
}

let tracks = [];
let primaryId = null;

async function poll(){
  try{
    const r = await fetch('/tracks', {cache:'no-store'});
    if(!r.ok) throw 0;
    tracks = await r.json();
    $('fTracks').textContent = tracks.length;
    if(tracks.length){
      // pega o mais recente ou o FA-18C / GRIPEN
      tracks.sort((a,b)=> new Date(b.lastUpdateUtc)-new Date(a.lastUpdateUtc));
      const t = tracks[0];
      primaryId = t.id;
      renderTrack(t);
      $('linkDot').className='dot on'; $('linkTxt').textContent='LINK DCS: ON';
    } else {
      $('linkDot').className='dot off'; $('linkTxt').textContent='LINK DCS: OFF';
    }
  } catch(e){
    $('linkDot').className='dot off'; $('linkTxt').textContent='LINK DCS: OFF';
  }
}

function renderTrack(t){
  const lat = t.latitude, lon = t.longitude, hdg = t.headingDegrees||0;
  // MDIs — valores crus, como no cockpit
  $('callsignHdr').textContent = t.callsign;
  $('fCallsign').textContent = t.callsign;
  $('fLat').textContent = fmt.coord(lat); $('fLon').textContent = fmt.coord(lon);
  $('latVal').textContent = fmt.coord(lat); $('lonVal').textContent = fmt.coord(lon);
  $('coordHud').textContent = `${fmt.coord(lat)}, ${fmt.coord(lon)}`;
  $('hdgVal').textContent = fmt.i(hdg); $('fHdg').textContent = fmt.i(hdg);
  const altMs = t.altitudeMeters||0, agl = t.altitudeAglMeters||0;
  $('altMsl').textContent = fmt.ft(altMs); $('altMs').textContent = fmt.i(altMs); $('altAgl').textContent = fmt.ft(agl); $('fAlt').textContent = fmt.i(altMs);
  const tas = t.speedMetersPerSecond||0, ias = t.indicatedAirSpeedMps||0;
  $('tas').textContent = fmt.i(tas); $('ias').textContent = fmt.kt(ias||tas); $('fSpd').textContent = fmt.i(tas);
  $('mach').textContent = fmt.f2(t.machNumber||0);
  $('vvi').textContent = fmt.i((t.verticalVelocityMps||0)*196.85);
  $('aoa').textContent = fmt.f1(t.angleOfAttackDeg||0);
  $('gload').textContent = fmt.f1(t.gLoad||1);
  $('pb').textContent = `${fmt.i(t.pitchDeg||0)}/${fmt.i(t.bankDeg||0)}`;
  $('hdgRose').style.transform = `rotate(${hdg}deg)`;
  const fuelInt = t.fuelInternalKg||0, fuelExt = t.fuelExternalKg||0, tot = fuelInt+fuelExt;
  $('fuelInt').textContent = fmt.i(fuelInt);
  $('fuelExt').textContent = fmt.i(fuelExt);
  $('fuelTot').textContent = fmt.i(tot);
  const pct = Math.min(100, tot/4500*100); // escala ~4500kg típico F-18C
  $('fuelIntBar').style.width = pct+"%";
  $('rpmL').textContent = fmt.f1(t.engineRpmLeft||0);
  $('rpmR').textContent = fmt.f1(t.engineRpmRight||0);
  if($('chaff')) $('chaff').textContent = "—"; if($('flare')) $('flare').textContent = "—";
  if($('stores')) $('stores').textContent = t.callsign+" — PAYLOAD (via LoGetPayloadInfo em breve)";
  if($('storesAAM')) $('storesAAM').textContent = "—";
  if($('storesAGM')) $('storesAGM').textContent = "—";
  if($('gearInd')) $('gearInd').textContent = "GEAR —"; if($('flapInd')) $('flapInd').textContent = "FLAP —"; if($('hookInd')) $('hookInd').textContent = "HOOK —";
  // IFEI / UFC (espelham valores crus)
  if($('ifeiRpmL')) $('ifeiRpmL').textContent = fmt.f1(t.engineRpmLeft||0);
  if($('ifeiRpmR')) $('ifeiRpmR').textContent = fmt.f1(t.engineRpmRight||0);
  if($('ifeiFuel')) $('ifeiFuel').textContent = fmt.i((t.fuelInternalKg||0)+(t.fuelExternalKg||0));
  if($('ifeiBingo')) $('ifeiBingo').textContent = fmt.i(t.fuelInternalKg||0);
  if($('ifeiTime')) $('ifeiTime').textContent = new Date(t.lastUpdateUtc).toLocaleTimeString('pt-BR');
  if($('ufcCallsign')) $('ufcCallsign').textContent = t.callsign;
  if($('ufcLat')) $('ufcLat').textContent = fmt.coord(t.latitude);
  if($('ufcLon')) $('ufcLon').textContent = fmt.coord(t.longitude);
  const st = (t.state===0?"TENTATIVE": t.state===1?"CONFIRMED" : t.state===2?"COASTING":"DROPPED");
  const sb = $('stateBox'); sb.textContent = st; sb.className = "state "+st.toLowerCase();
  $('fTime').textContent = new Date(t.lastUpdateUtc).toLocaleTimeString('pt-BR');

  // mapa
  if(isFinite(lat) && isFinite(lon)){
    if(!marker){
      marker = L.marker([lat,lon], {icon: acIcon(hdg)}).addTo(map);
      map.setView([lat,lon], 12);
    } else {
      marker.setLatLng([lat,lon]);
      marker.setIcon(acIcon(hdg));
    }
    trail.push([lat,lon]);
    if(trail.length>400) trail.shift();
    poly.setLatLngs(trail);
    $('trailInfo').textContent = `TRAIL ${trail.length} pts • ${t.callsign}`;
    $('coordHud').textContent = `${fmt.coord(lat)} , ${fmt.coord(lon)} • ${fmt.i(hdg)}°`;
  }
}

function clock(){ $('clockZ').textContent = new Date().toISOString().slice(11,19)+"Z"; }
setInterval(clock,1000); clock();
setInterval(poll, 800); poll();
