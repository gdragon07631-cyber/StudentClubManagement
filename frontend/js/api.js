import { API_BASE_URL } from './config.js';

const SESSION_KEY = 'scm.session';

export const Roles = { Admin: 'ADMIN', ClubLeader: 'CLUB_LEADER', Member: 'MEMBER' };

/** Phiên đăng nhập (JWT + thông tin người dùng) lưu trong localStorage. */
export const session = {
  get() {
    try {
      const s = JSON.parse(localStorage.getItem(SESSION_KEY) || 'null');
      if (!s || !s.accessToken) return null;
      if (s.expiresAtUtc && new Date(s.expiresAtUtc.endsWith('Z') ? s.expiresAtUtc : s.expiresAtUtc + 'Z') < new Date()) { this.clear(); return null; }
      return s;
    } catch { return null; }
  },
  set(s) { localStorage.setItem(SESSION_KEY, JSON.stringify(s)); },
  clear() { localStorage.removeItem(SESSION_KEY); },
  isInRole(role) { return !!this.get()?.roles?.includes(role); },
  get isAdmin() { return this.isInRole(Roles.Admin); },
  get isClubLeader() { return this.isInRole(Roles.ClubLeader); },
  get isMember() { return this.isInRole(Roles.Member); },
};

export class ApiError extends Error {
  constructor(status, message, data) { super(message); this.status = status; this.data = data; }
}

function errorMessage(status, data) {
  if (data && typeof data === 'object') {
    if (typeof data.message === 'string') return data.message;
    if (data.errors && typeof data.errors === 'object') return Object.values(data.errors).flat().join(' ');
    if (typeof data.detail === 'string') return data.detail;
    if (typeof data.title === 'string') return data.title;
  }
  return `Yêu cầu không thành công (${status}).`;
}

/** Ghép query string; bỏ qua giá trị null/rỗng. */
export function url(path, query) {
  const qs = new URLSearchParams();
  for (const [k, v] of Object.entries(query || {})) if (v !== null && v !== undefined && String(v).trim() !== '') qs.set(k, v);
  const s = qs.toString();
  return s ? `${path}?${s}` : path;
}

async function request(method, path, body) {
  const token = session.get()?.accessToken;
  const headers = { Accept: 'application/json' };
  if (token) headers.Authorization = `Bearer ${token}`;
  if (body !== undefined) headers['Content-Type'] = 'application/json';
  let res;
  try {
    res = await fetch(API_BASE_URL.replace(/\/$/, '') + '/' + path.replace(/^\//, ''), { method, headers, body: body === undefined ? undefined : JSON.stringify(body) });
  } catch {
    throw new ApiError(0, 'Không kết nối được máy chủ API. Vui lòng kiểm tra backend đang chạy.');
  }
  let data = null;
  const text = await res.text();
  if (text) { try { data = JSON.parse(text); } catch { data = null; } }
  if (res.ok) return data;
  // Phiên hết hạn: chỉ khi đã có token (đăng nhập sai cũng trả 401 nhưng không có token).
  if (res.status === 401 && token) {
    session.clear();
    sessionStorage.setItem('scm.flash', JSON.stringify({ type: 'danger', message: 'Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.' }));
    location.hash = '#/login';
  }
  throw new ApiError(res.status, errorMessage(res.status, data), data);
}

export const api = {
  get: (path, query) => request('GET', url(path, query)),
  post: (path, body) => request('POST', path, body ?? {}),
  put: (path, body) => request('PUT', path, body ?? {}),
  del: (path) => request('DELETE', path),
  url,
};

