// GripenLink — DDI/AMPCD com push SignalR (fallback: polling)
const $ = s => document.getElementById(s);
const set = (id, v) => { const e = $(id); if (e) e.textContent = v; };
const fmt = {
  f1: v => isFinite(v) ? v.toFixed(1) : "—",
  f2: v => isFinite(v) ? v.toFixed(2) : "—",
  i: v => isFinite(v) ? Math.round(v).toString() : "—",
  ft: m => isFinite(m) ? Math.round(m*3.28084).toString() : "—",
  kt: mps => isFinite(mps) ? (mps*1.94384).toFixed(0) : "—",
  coord: v => isFinite(v) ? v.toFixed(5) : "—",
};

// Leaflet — mapa escuro
const map = L.map('map', {zoomControl:false}).setView([-23.18,-45.86], 11);
L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {attribution:'© OSM', maxZoom:18}).addTo(map);
L.control.zoom({position:'topright'}).addTo(map);

const trail = [];
const poly = L.polyline([], {color:'#2de06a', weight:2, opacity:.9}).addTo(map);
let marker = null;

function acIcon(hdg){
  const el = document.createElement('div'); el.className='ac-marker';
  el.style.transform = `rotate(${hdg}deg)`;
  return L.divIcon({className:'', html: el, iconSize:[14,14], iconAnchor:[7,7]});
}

function setLink(on){
  set('linkTxt', on ? 'LINK DCS: ON' : 'LINK DCS: OFF');
  const d = $('linkDot'); if(d) d.className = on ? 'dot on' : 'dot off';
}

function renderTrack(t){
  const lat = t.latitude, lon = t.longitude, hdg = t.headingDegrees||0;
  set('callsignHdr', t.callsign);
  set('fCallsign', t.callsign);
  set('ufcCallsign', t.callsign);
  set('latVal', fmt.coord(lat)); set('lonVal', fmt.coord(lon));
  set('ufcLat', fmt.coord(lat)); set('ufcLon', fmt.coord(lon));
  set('coordHud', `${fmt.coord(lat)}, ${fmt.coord(lon)}`);
  set('hdgVal', fmt.i(hdg)); set('fHdg', fmt.i(hdg));
  set('hdgRose', ''); // mantém texto do ◇ (rotate via style abaixo)
  const rose = $('hdgRose'); if(rose) rose.style.transform = `rotate(${hdg}deg)`;

  const altMs = t.altitudeMeters||0, agl = t.altitudeAglMeters||0;
  set('altMsl', fmt.ft(altMs)); set('altAgl', fmt.ft(agl)); set('fAlt', fmt.i(altMs));
  const tas = t.speedMetersPerSecond||0, ias = t.indicatedAirSpeedMps||0;
  set('tas', fmt.i(tas)); set('ias', fmt.kt(ias||tas)); set('fSpd', fmt.i(tas));
  set('mach', fmt.f2(t.machNumber||0));
  set('vvi', fmt.i((t.verticalVelocityMps||0)*196.85));
  set('aoa', fmt.f1(t.angleOfAttackDeg||0));
  set('gload', fmt.f1(t.gLoad||1));
  set('pb', `${fmt.i(t.pitchDeg||0)}/${fmt.i(t.bankDeg||0)}`);

  const fuelInt = t.fuelInternalKg||0, fuelExt = t.fuelExternalKg||0, tot = fuelInt+fuelExt;
  set('fuelInt', fmt.i(fuelInt)); set('fuelExt', fmt.i(fuelExt)); set('fuelTot', fmt.i(tot));
  set('ifeiFuel', fmt.i(tot)); set('ifeiBingo', fmt.i(fuelInt));
  const bar = $('fuelIntBar'); if(bar) bar.style.width = Math.min(100, tot/4500*100)+"%";
  set('rpmL', fmt.f1(t.engineRpmLeft||0)); set('rpmR', fmt.f1(t.engineRpmRight||0));
  set('ifeiRpmL', fmt.f1(t.engineRpmLeft||0)); set('ifeiRpmR', fmt.f1(t.engineRpmRight||0));

  const st = (t.state===0?"TENTATIVE": t.state===1?"CONFIRMED" : t.state===2?"COASTING":"DROPPED");
  const sb = $('stateBox'); if(sb){ sb.textContent = st; sb.className = "state "+st.toLowerCase(); }
  set('fTime', new Date(t.lastUpdateUtc).toLocaleTimeString('pt-BR'));
  set('ifeiTime', new Date(t.lastUpdateUtc).toLocaleTimeString('pt-BR'));

  // mapa
  if(isFinite(lat) && isFinite(lon)){
    if(!marker){ marker = L.marker([lat,lon], {icon: acIcon(hdg)}).addTo(map); map.setView([lat,lon], 12); }
    else { marker.setLatLng([lat,lon]); marker.setIcon(acIcon(hdg)); }
    trail.push([lat,lon]); if(trail.length>400) trail.shift();
    poly.setLatLngs(trail);
    set('trailInfo', `TRAIL ${trail.length} pts • ${t.callsign}`);
    set('coordHud', `${fmt.coord(lat)} , ${fmt.coord(lon)} • ${fmt.i(hdg)}°`);
  }
}

function renderAll(tracks){
  set('fTracks', tracks.length);
  if(!tracks.length){ setLink(false); return; }
  tracks.sort((a,b)=> new Date(b.lastUpdateUtc)-new Date(a.lastUpdateUtc));
  renderTrack(tracks[0]);
  setLink(true);
}

// Polling (fallback + carga inicial)
async function poll(){
  try{
    const r = await fetch('/tracks', {cache:'no-store'});
    if(!r.ok) throw 0;
    renderAll(await r.json());
  }catch(e){ setLink(false); }
}

// SignalR (push em tempo real)
function startSignalR(){
  if(typeof signalR === 'undefined'){ setInterval(poll, 300); return; }
  const conn = new signalR.HubConnectionBuilder().withUrl('/tracksHub').withAutomaticReconnect().build();
  conn.on('TracksUpdate', renderAll);
  conn.onreconnected(() => poll());
  conn.start().then(() => setLink(true)).catch(() => { setInterval(poll, 300); });
}

function clock(){ set('clockZ', new Date().toISOString().slice(11,19)+'Z'); }
setInterval(clock,1000); clock();
poll();
startSignalR();
