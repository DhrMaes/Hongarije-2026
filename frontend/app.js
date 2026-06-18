/* ── CONFIG ── */
// In the dev container (nginx on :3000) and in Docker, /api is proxied by nginx.
// When opening index.html directly via Live Server (:5500), call the API directly.
const API = window.location.port === '5500' ? 'http://localhost:5000/api' : '/api';

const MAPS_LINK  = 'https://maps.app.goo.gl/TnpmEyBhF4tUNtbU7';
const HOUSE_LINK = 'https://www.interhome.be/rental/9fef3e704583bf14177d5eae79bb0efd';
const WAZE_LINK  = 'https://ul.waze.com/ul?venue_id=11665876.116527689.11321776&overview=yes&utm_campaign=default&utm_source=waze_website&utm_medium=lm_share_location';
const VIGNET_AT  = 'https://shop.asfinag.at/nl/';
const VIGNET_HU  = 'https://ematrica.nemzetiutdij.hu/en/vignette-purchase';

const FOOD_CATEGORIES = [
  { value: 'zelf-koken',    label: 'Zelf koken' },
  { value: 'aan-het-meer',  label: 'Aan het meer' },
  { value: 'op-restaurant', label: 'Op restaurant' },
];

let me = null;
let meIsAdmin = false;
let allUsers = [];
let skipAutoLogin = false;

/* ── HELPERS ── */
function esc(s) {
  return String(s ?? '').replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
}

function ini(n) {
  return (n ?? '?').split(' ').map(w => w[0]).join('').toUpperCase().slice(0, 2);
}

function isAdmin() { return meIsAdmin; }
function isAdminUser(name) { return allUsers.find(u => u.name === name)?.isAdmin ?? false; }

function foodCatLabel(val) {
  return FOOD_CATEGORIES.find(c => c.value === val)?.label ?? FOOD_CATEGORIES[0].label;
}

function toast(msg, durationMs = 2200) {
  const t = document.getElementById('toast');
  t.textContent = msg;
  t.classList.add('show');
  clearTimeout(t._timer);
  t._timer = setTimeout(() => t.classList.remove('show'), durationMs);
}

function toastError(msg) {
  toast('⚠️ ' + msg, 5000);
}

/* ── API ── */
async function apiFetch(path, options = {}) {
  let res;
  try {
    res = await fetch(API + path, {
      headers: { 'Content-Type': 'application/json' },
      ...options,
      body: options.body ? JSON.stringify(options.body) : undefined,
    });
  } catch (err) {
    throw new Error(`Netwerkfout — kan de server niet bereiken (${path}): ${err.message}`);
  }
  if (!res.ok) {
    throw new Error(`Serverfout ${res.status} op ${path}`);
  }
  if (res.status === 204 || res.headers.get('content-length') === '0') return null;
  const text = await res.text();
  return text ? JSON.parse(text) : null;
}

function renderError(el, err) {
  el.innerHTML = `<div class="empty"><p>⚠️ ${esc(err.message)}</p></div>`;
  console.error(err);
}

/* ── NICKNAME ── */
async function renderChips() {
  try {
    allUsers = await apiFetch('/users');
  } catch (err) {
    console.warn('Kon bestaande namen niet laden:', err.message);
    return;
  }
  const bl = document.getElementById('existing-block');
  const ch = document.getElementById('name-chips');
  if (!allUsers.length) { bl.style.display = 'none'; return; }
  bl.style.display = 'block';
  ch.innerHTML = allUsers.map(u => `<span class="name-chip" onclick="pickName('${esc(u.name)}')">${esc(u.name)}</span>`).join('');
}

function pickName(n) {
  document.getElementById('name-input').value = n;
  startApp();
}

async function startApp() {
  const val = document.getElementById('name-input').value.trim();
  if (!val) { document.getElementById('name-input').focus(); return; }

  // If this user has a PIN set, verify it before proceeding
  const existingUser = allUsers.find(u => u.name === val);
  if (existingUser?.hasPin) {
    const verified = await promptPin(val);
    if (!verified) return;
  }

  const btn = document.querySelector('.btn-go');
  btn.disabled = true;
  btn.textContent = 'Even wachten…';
  try {
    me = val;
    const user = await apiFetch('/users', { method: 'POST', body: { name: val } });
    meIsAdmin = user.isAdmin ?? false;
  } catch {
    me = null;
    btn.disabled = false;
    btn.textContent = 'Verder →';
    return;
  }
  document.getElementById('nickname-screen').style.display = 'none';
  document.getElementById('app').style.display = 'block';
  updateHeader();
  await renderAll();
}

function switchUser() {
  me = null;
  meIsAdmin = false;
  skipAutoLogin = true;
  document.getElementById('app').style.display = 'none';
  document.getElementById('nickname-screen').style.display = 'flex';
  document.getElementById('name-input').value = '';
  renderChips();
}

function updateHeader() {
  document.getElementById('hdr-name').textContent = me;
  const av = document.getElementById('hdr-av');
  av.textContent = ini(me);
  av.classList.toggle('is-admin', isAdmin());
  document.getElementById('hdr-admin').style.display = isAdmin() ? 'inline' : 'none';
  document.getElementById('hdr-pin-btn').style.display = isAdmin() ? 'inline' : 'none';
  document.getElementById('itinerary-add-btn').innerHTML =
    `<button class="btn-add" onclick="openModal('itinerary')">+ Eetidee toevoegen</button>`;
  document.getElementById('info-add-btn').innerHTML = isAdmin()
    ? `<button class="btn-add" onclick="openModal('info')">+ Info toevoegen</button>` : '';
}

/* ── TABS ── */
function showTab(name, btn) {
  document.querySelectorAll('.tab-panel').forEach(p => p.classList.remove('active'));
  document.querySelectorAll('.tab-btn').forEach(b => b.classList.remove('active'));
  document.getElementById('tab-' + name).classList.add('active');
  btn.classList.add('active');
}

/* ── MODALS ── */
function openModal(t) {
  document.getElementById('modal-' + t).classList.add('open');
  setTimeout(() => {
    const f = document.querySelector(`#modal-${t} input, #modal-${t} textarea`);
    if (f) f.focus();
  }, 50);
}

function closeModal(t) {
  document.getElementById('modal-' + t).classList.remove('open');
  document.querySelectorAll(`#modal-${t} input, #modal-${t} textarea, #modal-${t} select`).forEach(el => {
    el.value = el.tagName === 'SELECT' ? (el.options[0]?.value ?? '') : '';
  });
}

function confirmDialog(message) {
  return new Promise(resolve => {
    document.getElementById('confirm-msg').textContent = message;
    openModal('confirm');
    const ok     = document.getElementById('confirm-ok');
    const cancel = document.getElementById('confirm-cancel');
    function cleanup(result) {
      closeModal('confirm');
      ok.removeEventListener('click', onOk);
      cancel.removeEventListener('click', onCancel);
      resolve(result);
    }
    function onOk()     { cleanup(true);  }
    function onCancel() { cleanup(false); }
    ok.addEventListener('click', onOk);
    cancel.addEventListener('click', onCancel);
  });
}

document.querySelectorAll('.modal-overlay').forEach(o => {
  o.addEventListener('click', e => { if (e.target === o) closeModal(o.id.replace('modal-', '')); });
});
document.addEventListener('keydown', e => {
  if (e.key === 'Escape')
    document.querySelectorAll('.modal-overlay.open').forEach(o => closeModal(o.id.replace('modal-', '')));
});

// ── PIN entry (wissel to protected account) ────────────────────────────────

let _pinEntryResolve = null;

function promptPin(name) {
  return new Promise(resolve => {
    _pinEntryResolve = resolve;
    document.getElementById('pin-entry-name').textContent = name;
    document.getElementById('pin-entry-input').value = '';
    document.getElementById('pin-entry-error').style.display = 'none';
    document.getElementById('modal-pin-entry').classList.add('open');
    setTimeout(() => document.getElementById('pin-entry-input').focus(), 100);
  });
}

async function submitPinEntry() {
  const name = document.getElementById('pin-entry-name').textContent;
  const pin = document.getElementById('pin-entry-input').value;
  try {
    await apiFetch(`/users/${encodeURIComponent(name)}/verify-pin`, {
      method: 'POST', body: { pin }
    });
    document.getElementById('modal-pin-entry').classList.remove('open');
    if (_pinEntryResolve) { _pinEntryResolve(true); _pinEntryResolve = null; }
  } catch {
    document.getElementById('pin-entry-error').style.display = 'block';
    document.getElementById('pin-entry-input').select();
  }
}

function cancelPinEntry() {
  document.getElementById('modal-pin-entry').classList.remove('open');
  if (_pinEntryResolve) { _pinEntryResolve(false); _pinEntryResolve = null; }
}

// ── PIN manage (set / change / remove own PIN) ─────────────────────────────

function openPinManage() {
  const user = allUsers.find(u => u.name === me);
  const hasPin = user?.hasPin ?? false;
  document.getElementById('pin-manage-title').textContent = hasPin ? 'PIN wijzigen' : 'PIN instellen';
  document.getElementById('pin-current-group').style.display = hasPin ? 'block' : 'none';
  document.getElementById('pin-remove-btn').style.display = hasPin ? 'inline' : 'none';
  document.getElementById('pin-current').value = '';
  document.getElementById('pin-new').value = '';
  document.getElementById('pin-confirm').value = '';
  document.getElementById('pin-manage-error').style.display = 'none';
  document.getElementById('modal-pin-manage').classList.add('open');
}

async function savePin() {
  const currentPin = document.getElementById('pin-current').value;
  const newPin = document.getElementById('pin-new').value;
  const confirmPin = document.getElementById('pin-confirm').value;
  const errEl = document.getElementById('pin-manage-error');
  errEl.style.display = 'none';

  if (newPin.length < 4) { errEl.textContent = 'PIN moet minimaal 4 tekens zijn.'; errEl.style.display = 'block'; return; }
  if (newPin !== confirmPin) { errEl.textContent = 'PINs komen niet overeen.'; errEl.style.display = 'block'; return; }

  const user = allUsers.find(u => u.name === me);
  const body = { newPin };
  if (user?.hasPin) body.currentPin = currentPin;

  try {
    await apiFetch(`/users/${encodeURIComponent(me)}/pin`, { method: 'POST', body });
    closeModal('pin-manage');
    allUsers = await apiFetch('/users') ?? allUsers;
    showToast('PIN opgeslagen ✓');
  } catch (e) {
    errEl.textContent = e.message || 'Fout bij opslaan PIN.';
    errEl.style.display = 'block';
  }
}

async function removePin() {
  const pin = document.getElementById('pin-current').value;
  const errEl = document.getElementById('pin-manage-error');
  try {
    await apiFetch(`/users/${encodeURIComponent(me)}/pin`, { method: 'DELETE', body: { pin } });
    closeModal('pin-manage');
    allUsers = await apiFetch('/users') ?? allUsers;
    showToast('PIN verwijderd.');
  } catch (e) {
    errEl.textContent = e.message || 'Verkeerde PIN.';
    errEl.style.display = 'block';
  }
}

/* ── WISHLIST ── */
async function saveWishlist() {
  const title = document.getElementById('w-title').value.trim();
  if (!title) { document.getElementById('w-title').focus(); return; }
  await apiFetch('/wishlist', {
    method: 'POST',
    body: { title, description: document.getElementById('w-desc').value.trim(), link: document.getElementById('w-link').value.trim(), author: me },
  });
  closeModal('wishlist');
  await renderWishlist();
  toast('Activiteit toegevoegd! 🏖️');
}

async function vote(id, dir) {
  await apiFetch(`/wishlist/${id}/vote`, { method: 'POST', body: { user: me, direction: dir } });
  await renderWishlist();
}

async function delWishlist(id, author) {
  if (!isAdmin() && author !== me) return;
  if (!await confirmDialog('Weet je zeker dat je dit wilt verwijderen?')) return;
  await apiFetch(`/wishlist/${id}`, { method: 'DELETE' });
  await renderWishlist();
  toast('Verwijderd.');
}

async function renderWishlist() {
  const el = document.getElementById('wishlist-list');
  let items;
  try {
    items = await apiFetch('/wishlist');
  } catch (err) {
    renderError(el, err);
    return;
  }
  if (!items.length) {
    el.innerHTML = `<div class="empty"><div class="ei">🏖️</div><p>Nog geen activiteiten, voeg de eerste toe!</p></div>`;
    return;
  }
  el.innerHTML = items.map(item => {
    const ups      = (item.votes ?? []).filter(v => v.direction === 'up');
    const downs    = (item.votes ?? []).filter(v => v.direction === 'down');
    const upNames  = ups.map(v => v.userName).join(', ');
    const myVote   = (item.votes ?? []).find(v => v.userName === me)?.direction;
    const isAdm    = isAdminUser(item.author);
    return `<div class="card">
      <div style="display:flex;align-items:flex-start;justify-content:space-between;gap:.5rem">
        <div style="flex:1">
          <div class="card-title">${esc(item.title)}</div>
          ${item.description ? `<div class="card-desc">${esc(item.description)}</div>` : ''}
        </div>
        ${isAdmin() || item.author === me ? `<button class="btn-icon del" onclick="delWishlist('${item.id}','${esc(item.author)}')" title="Verwijderen">✕</button>` : ''}
      </div>
      <div class="card-meta">
        <span class="author-tag"><span class="av${isAdm ? ' admin' : ''}">${ini(item.author)}</span>${esc(item.author)}</span>
        ${item.link ? `<a class="link-tag" href="${esc(item.link)}" target="_blank">🔗 link</a>` : ''}
      </div>
      <div class="vote-row">
        <button class="btn-vote${myVote === 'up' ? ' yes' : ''}" onclick="vote('${item.id}','up')">👍 Ja! ${ups.length || ''}</button>
        <button class="btn-vote${myVote === 'down' ? ' no' : ''}" onclick="vote('${item.id}','down')">👎 Nee ${downs.length || ''}</button>
        ${upNames ? `<span class="vote-who">✓ ${esc(upNames)}</span>` : ''}
      </div>
    </div>`;
  }).join('');
}

/* ── ITINERARY ── */
async function saveItinerary() {
  const title = document.getElementById('i-title').value.trim();
  if (!title) { document.getElementById('i-title').focus(); return; }
  await apiFetch('/itinerary', {
    method: 'POST',
    body: { title, category: document.getElementById('i-cat').value, description: document.getElementById('i-desc').value.trim(), author: me },
  });
  closeModal('itinerary');
  await renderItinerary();
  toast('Eetidee toegevoegd! 🍽️');
}

async function voteItinerary(id, dir) {
  await apiFetch(`/itinerary/${id}/vote`, { method: 'POST', body: { user: me, direction: dir } });
  await renderItinerary();
}

async function delItinerary(id, author) {
  if (!isAdmin() && author !== me) return;
  if (!await confirmDialog('Weet je zeker dat je dit wilt verwijderen?')) return;
  await apiFetch(`/itinerary/${id}`, { method: 'DELETE' });
  await renderItinerary();
  toast('Verwijderd.');
}

async function renderItinerary() {
  const el = document.getElementById('itinerary-days');
  let items;
  try {
    items = await apiFetch('/itinerary');
  } catch (err) {
    renderError(el, err);
    return;
  }
  if (!items.length) {
    el.innerHTML = `<div class="empty"><div class="ei">🍽️</div><p>Nog geen eetideeën, voeg de eerste toe!</p></div>`;
    return;
  }
  const grouped = Object.fromEntries(FOOD_CATEGORIES.map(c => [c.value, []]));
  items.forEach(item => {
    const cat = grouped[item.category] ? item.category : FOOD_CATEGORIES[0].value;
    grouped[cat].push(item);
  });
  el.innerHTML = FOOD_CATEGORIES.map(({ value }) => {
    const catItems = grouped[value];
    if (!catItems.length) return '';
    return `<div class="day-group">
      <div class="day-header">${esc(foodCatLabel(value))}</div>
      <div class="cards-grid">
        ${catItems.map(item => {
          const ups     = (item.votes ?? []).filter(v => v.direction === 'up');
          const downs   = (item.votes ?? []).filter(v => v.direction === 'down');
          const upNames = ups.map(v => v.userName).join(', ');
          const myVote  = (item.votes ?? []).find(v => v.userName === me)?.direction;
          const isAdm   = isAdminUser(item.author);
          return `<div class="card">
            <div style="display:flex;align-items:flex-start;justify-content:space-between;gap:.5rem">
              <div style="flex:1">
                <div class="card-title">${esc(item.title)}</div>
                ${item.description ? `<div class="card-desc">${esc(item.description)}</div>` : ''}
              </div>
              ${isAdmin() || item.author === me ? `<button class="btn-icon del" onclick="delItinerary('${item.id}','${esc(item.author)}')" title="Verwijderen">✕</button>` : ''}
            </div>
            <div class="card-meta">
              <span class="category-tag">${esc(foodCatLabel(item.category))}</span>
              <span class="author-tag"><span class="av${isAdm ? ' admin' : ''}">${ini(item.author)}</span>${esc(item.author)}</span>
            </div>
            <div class="vote-row">
              <button class="btn-vote${myVote === 'up' ? ' yes' : ''}" onclick="voteItinerary('${item.id}','up')">👍 Ja! ${ups.length || ''}</button>
              <button class="btn-vote${myVote === 'down' ? ' no' : ''}" onclick="voteItinerary('${item.id}','down')">👎 Nee ${downs.length || ''}</button>
              ${upNames ? `<span class="vote-who">✓ ${esc(upNames)}</span>` : ''}
            </div>
          </div>`;
        }).join('')}
      </div>
    </div>`;
  }).filter(Boolean).join('');
}

/* ── PACKING ── */
async function savePacking() {
  const title = document.getElementById('p-title').value.trim();
  if (!title) { document.getElementById('p-title').focus(); return; }
  await apiFetch('/packing', {
    method: 'POST',
    body: { title, category: document.getElementById('p-cat').value, owner: me },
  });
  closeModal('packing');
  await renderPacking();
  toast('Valiesitem toegevoegd! 🧳');
}

async function togglePack(id) {
  await apiFetch(`/packing/${id}/toggle`, { method: 'PATCH' });
  await renderPacking();
}

async function delPack(id, title) {
  if (!await confirmDialog(`Weet je zeker dat je "${title}" wilt verwijderen?`)) return;
  await apiFetch(`/packing/${id}`, { method: 'DELETE' });
  await renderPacking();
  toast('Verwijderd.');
}

async function saveSharedPacking() {
  const title = document.getElementById('sp-title').value.trim();
  if (!title) { document.getElementById('sp-title').focus(); return; }
  await apiFetch('/packing/shared', {
    method: 'POST',
    body: { title, note: document.getElementById('sp-note').value.trim(), addedBy: me },
  });
  closeModal('shared-pack');
  await renderPacking();
  toast('Gemeenschappelijk item toegevoegd! 🌍');
}

async function toggleSharedPack(id) {
  await apiFetch(`/packing/shared/${id}/toggle`, { method: 'PATCH', body: { packedBy: me } });
  await renderPacking();
}

async function delSharedPack(id, title) {
  if (!await confirmDialog(`Weet je zeker dat je "${title}" wilt verwijderen?`)) return;
  await apiFetch(`/packing/shared/${id}`, { method: 'DELETE' });
  await renderPacking();
  toast('Verwijderd.');
}

async function renderPacking() {
  const el = document.getElementById('packing-cols');
  let myItems, sharedItems;
  try {
    [myItems, sharedItems] = await Promise.all([
      apiFetch(`/packing?user=${encodeURIComponent(me)}`),
      apiFetch('/packing/shared'),
    ]);
  } catch (err) {
    renderError(el, err);
    return;
  }

  const packed = myItems.filter(i => i.isPacked).length;
  const pct    = myItems.length ? Math.round(packed / myItems.length * 100) : 0;
  const byCat  = {};
  myItems.forEach(i => { (byCat[i.category] ??= []).push(i); });
  const isAdm  = isAdmin();

  const personalHtml = myItems.length
    ? `<div class="packing-person-card">
        <div class="packing-person-name">
          <span class="av${isAdm ? ' admin' : ''}">${ini(me)}</span>${esc(me)}
          <span style="margin-left:auto;font-size:.72rem;color:var(--faint);font-weight:400">${packed}/${myItems.length}</span>
        </div>
        <div class="pack-progress"><div class="pack-progress-fill" style="width:${pct}%"></div></div>
        <p class="section-desc" style="margin:0.65rem 0 0.5rem">Deze lijst is alleen zichtbaar voor jouw profiel.</p>
        <button class="btn-add" onclick="openModal('packing')" style="margin-bottom:0.65rem">+ Persoonlijk item toevoegen</button>
        ${Object.entries(byCat).map(([cat, citems]) => `
          <div class="pack-cat-label">${esc(cat)}</div>
          ${citems.map(item => `
            <div class="pack-item${item.isPacked ? ' done' : ''}">
              <input type="checkbox" id="pk-${item.id}" ${item.isPacked ? 'checked' : ''} onchange="togglePack('${item.id}')" />
              <label for="pk-${item.id}">${esc(item.title)}</label>
              ${isAdmin() || item.owner === me ? `<button class="btn-icon del" onclick="delPack('${item.id}','${esc(item.title)}')" style="width:22px;height:22px;font-size:.7rem">✕</button>` : ''}
            </div>`).join('')}
        `).join('')}
      </div>`
    : `<div class="packing-person-card">
        <button class="btn-add" onclick="openModal('packing')" style="margin-bottom:0.65rem">+ Persoonlijk item toevoegen</button>
        <div class="empty" style="padding:1rem"><div class="ei">🧳</div><p>Je valies is nog leeg — voeg je eerste item toe!</p></div>
      </div>`;

  const sortedShared = [...sharedItems].sort((a, b) => (a.isPacked ? 1 : 0) - (b.isPacked ? 1 : 0));
  const sharedPacked = sortedShared.filter(i => i.isPacked).length;

  const sharedHtml = `<div class="packing-person-card">
    <div class="packing-person-name">
      <span class="av">🌍</span>Gemeenschappelijk
      <span style="margin-left:auto;font-size:.72rem;color:var(--faint);font-weight:400">${sharedPacked}/${sortedShared.length}</span>
    </div>
    <p class="section-desc" style="margin:0.15rem 0 0.5rem">Deze items zijn zichtbaar voor iedereen.</p>
    <button class="btn-add" onclick="openModal('shared-pack')" style="margin-bottom:0.65rem">+ Gemeenschappelijk item</button>
    ${sortedShared.length
      ? sortedShared.map(item => `
          <div class="pack-item${item.isPacked ? ' done' : ''}">
            <input type="checkbox" id="spk-${item.id}" ${item.isPacked ? 'checked' : ''} onchange="toggleSharedPack('${item.id}')" />
            <label for="spk-${item.id}">
              ${esc(item.title)}
              ${item.note    ? `<span class="pack-note">${esc(item.note)}</span>` : ''}
              ${item.packedBy ? `<span class="pack-owner">Mee door: ${esc(item.packedBy)}</span>` : ''}
            </label>
            ${isAdmin() || item.addedBy === me ? `<button class="btn-icon del" onclick="delSharedPack('${item.id}','${esc(item.title)}')" style="width:22px;height:22px;font-size:.7rem">✕</button>` : ''}
          </div>`).join('')
      : `<div class="empty" style="padding:1rem 0.4rem"><p>Nog geen gemeenschappelijke items.</p></div>`}
  </div>`;

  el.innerHTML = personalHtml + sharedHtml;
}

/* ── SHOPPING ── */
async function saveShopping() {
  const title = document.getElementById('s-title').value.trim();
  if (!title) { document.getElementById('s-title').focus(); return; }
  await apiFetch('/shopping', {
    method: 'POST',
    body: { title, description: document.getElementById('s-desc').value.trim(), author: me },
  });
  closeModal('shopping');
  await renderShopping();
  toast('Boodschap toegevoegd! 🛒');
}

async function toggleBought(id) {
  await apiFetch(`/shopping/${id}/toggle`, { method: 'PATCH' });
  await renderShopping();
}

async function delShopping(id, author) {
  if (!isAdmin() && author !== me) return;
  if (!await confirmDialog('Weet je zeker dat je dit wilt verwijderen?')) return;
  await apiFetch(`/shopping/${id}`, { method: 'DELETE' });
  await renderShopping();
  toast('Verwijderd.');
}

async function renderShopping() {
  const el = document.getElementById('shopping-list');
  let items;
  try {
    items = await apiFetch('/shopping');
  } catch (err) {
    renderError(el, err);
    return;
  }
  if (!items.length) {
    el.innerHTML = `<div class="empty"><div class="ei">🛒</div><p>Nog geen boodschappen, voeg je favorieten toe!</p></div>`;
    return;
  }

  const SHOPPING_CATEGORIES = {
    'Fruit & groenten': /appel|banaan|fruit|aardbei|peer|druif|sinaasappel|kiwi|mango|tomaat|komkommer|sla|spinazie|broccoli|worteltje|aardappel|groent|paprika|ui|knoflook/,
    'Zuivel':           /melk|yoghurt|kaas|boter|crème|ijs|zuivel|ei|eieren/,
    'Vlees & vis':      /vlees|kip|hesp|bacon|worst|vis|garnaal|zalm/,
    'Brood':            /brood|stokbrood|baguette|pasta|rijst|granen|meel|tarwe/,
    'Drank':            /bier|wijn|water|fruitsap|frisdrank|thee|koffie|cappuccino|drank|cola|limonade/,
    'Snacks':           /chips|koekje|chocolade|snoep|noot|pinda|snack/,
  };

  const buckets = { ...Object.fromEntries(Object.keys(SHOPPING_CATEGORIES).map(k => [k, []])), 'Overige': [] };
  items.forEach(item => {
    const t = item.title.toLowerCase();
    const cat = Object.entries(SHOPPING_CATEGORIES).find(([, rx]) => rx.test(t))?.[0] ?? 'Overige';
    buckets[cat].push(item);
  });

  el.innerHTML = Object.entries(buckets).map(([cat, catItems]) => {
    if (!catItems.length) return '';
    catItems.sort((a, b) => (a.isBought ? 1 : 0) - (b.isBought ? 1 : 0));
    return `<div class="shop-cat-group">
      <div class="shop-cat-title">${esc(cat)}</div>
      <div class="shop-items">
        ${catItems.map(item => `
          <div class="shop-item${item.isBought ? ' checked' : ''}">
            <input type="checkbox" id="sh-${item.id}" ${item.isBought ? 'checked' : ''} onchange="toggleBought('${item.id}')" />
            <label for="sh-${item.id}">
              <div class="shop-item-title">${esc(item.title)}</div>
              ${item.description ? `<div class="shop-item-qty">${esc(item.description)}</div>` : ''}
            </label>
            ${isAdmin() || item.author === me ? `<button class="btn-icon del" onclick="delShopping('${item.id}','${esc(item.author)}')" style="width:22px;height:22px;font-size:.7rem">✕</button>` : ''}
          </div>`).join('')}
      </div>
    </div>`;
  }).filter(Boolean).join('');
}

/* ── INFO ── */
async function saveInfo() {
  if (!isAdmin()) return;
  const title = document.getElementById('n-title').value.trim();
  if (!title) { document.getElementById('n-title').focus(); return; }
  await apiFetch('/info', {
    method: 'POST',
    body: { category: document.getElementById('n-cat').value, title, body: document.getElementById('n-body').value.trim() },
  });
  closeModal('info');
  await renderInfo();
  toast('Info opgeslagen! 📌');
}

async function delInfo(id) {
  if (!isAdmin()) return;
  await apiFetch(`/info/${id}`, { method: 'DELETE' });
  await renderInfo();
  toast('Verwijderd.');
}

async function initCurrencyCalc(elemId) {
  const eur  = document.getElementById(`${elemId}-eur`);
  const huf  = document.getElementById(`${elemId}-huf`);
  const lbl  = document.getElementById(`${elemId}-rate-label`);
  if (!eur || !huf) return;

  let rate = 410;
  try {
    const data = await fetch('https://api.frankfurter.app/latest?from=EUR&to=HUF').then(r => r.json());
    rate = data.rates.HUF;
    if (lbl) lbl.textContent = `HUF (koers ${rate.toFixed(0)})`;
  } catch {
    if (lbl) lbl.textContent = `HUF (koers ~${rate} — offline)`;
  }

  eur.addEventListener('input', () => { huf.value = (parseFloat(eur.value) * rate).toFixed(0); });
  huf.addEventListener('input', () => { eur.value = (parseFloat(huf.value) / rate).toFixed(2); });
}

async function renderInfo() {
  const el = document.getElementById('info-list');
  let items;
  try {
    items = await apiFetch('/info');
  } catch (err) {
    renderError(el, err);
    return;
  }
  if (!items.length) {
    el.innerHTML = `<div class="empty"><div class="ei">📌</div><p>${isAdmin() ? 'Voeg praktische info toe, slaapplaatsen, adressen, tijden…' : 'De beheerder voegt hier praktische info toe.'}</p></div>`;
    return;
  }

  el.innerHTML = items.map(item => {
    const titleLink = item.link || (item.title === 'Appartement Moni 1' ? HOUSE_LINK : '');
    let specialContent = '';
    if (item.special === 'maps') {
      specialContent = `<div class="info-buttons">
        <a href="${MAPS_LINK}" target="_blank" class="info-btn">📍 Google Maps</a>
        <a href="${WAZE_LINK}" target="_blank" class="info-btn">🚗 Waze</a>
      </div>`;
    } else if (item.special === 'vignets') {
      specialContent = `<div class="vignet-grid">
        <a href="${VIGNET_AT}" target="_blank" class="vignet-link"><span class="vignet-emoji">🇦🇹</span><span>Vignet Oostenrijk</span></a>
        <a href="${VIGNET_HU}" target="_blank" class="vignet-link"><span class="vignet-emoji">🇭🇺</span><span>Vignet Hongarije</span></a>
      </div>`;
    } else if (item.special === 'currency') {
      const elemId = `calc-${item.id}`;
      setTimeout(() => initCurrencyCalc(elemId), 100);
      specialContent = `<div class="currency-calc">
        <div class="currency-input">
          <label>€ Euro</label>
          <input type="number" id="${elemId}-eur" placeholder="0.00" step="0.01" />
          <div class="currency-rate">EUR</div>
        </div>
        <div class="currency-input">
          <label>Ft Forint</label>
          <input type="number" id="${elemId}-huf" placeholder="0" step="1" />
          <div class="currency-rate" id="${elemId}-rate-label">HUF (koers ~410)</div>
        </div>
      </div>`;
    }

    return `<div class="info-card">
      <div style="display:flex;align-items:flex-start;justify-content:space-between;gap:.5rem">
        <div style="flex:1">
          <div class="info-cat">${esc(item.category)}</div>
          <div class="info-title-row">
            <div class="info-title">${esc(item.title)}</div>
            ${titleLink ? `<a href="${esc(titleLink)}" target="_blank" rel="noreferrer" class="info-link-icon" aria-label="Open ${esc(item.title)}">🏠</a>` : ''}
          </div>
          ${item.body ? `<div class="info-body">${esc(item.body)}</div>` : ''}
          ${specialContent}
        </div>
        ${isAdmin() ? `<button class="btn-icon del" onclick="delInfo('${item.id}')" title="Verwijderen">✕</button>` : ''}
      </div>
    </div>`;
  }).join('');
}

/* ── INIT ── */
async function renderAll() {
  await Promise.all([
    renderWishlist(),
    renderItinerary(),
    renderPacking(),
    renderShopping(),
    renderInfo(),
  ]);
}

async function tryAutoLogin() {
  if (skipAutoLogin) return;
  try {
    const user = await apiFetch('/auth/me');
    if (!user || !user.name) return;
    me = user.name;
    meIsAdmin = user.isAdmin ?? false;
    document.getElementById('nickname-screen').style.display = 'none';
    document.getElementById('app').style.display = 'block';
    updateHeader();
    await renderAll();
  } catch {
    // No Authentik header or API not reachable — fall through to login screen
  }
}

renderChips();
tryAutoLogin();
