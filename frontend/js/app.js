import { router, start } from './router.js';
import { register as account } from './pages/account.js';
import { register as dashboard } from './pages/dashboard.js';
import { register as clubs } from './pages/clubs.js';
import { register as members } from './pages/members.js';
import { register as applications } from './pages/applications.js';
import { register as users } from './pages/users.js';
import { register as activities } from './pages/activities.js';
import { register as meetings } from './pages/meetings.js';
import { register as points } from './pages/points.js';
import { register as tasks } from './pages/tasks.js';
import { register as finance } from './pages/finance.js';
import { register as reports } from './pages/reports.js';
import { register as notifications } from './pages/notifications.js';
import { register as ai } from './pages/ai.js';

[account, dashboard, clubs, members, applications, users, activities, meetings, points, tasks, finance, reports, notifications, ai]
  .forEach(register => register(router));

start();

