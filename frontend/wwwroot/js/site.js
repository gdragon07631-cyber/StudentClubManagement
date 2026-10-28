(() => {
  const body = document.body;
  const openButton = document.querySelector('.mobile-menu');
  const closeButton = document.querySelector('.sidebar-close');
  const backdrop = document.querySelector('.sidebar-backdrop');

  const closeSidebar = () => body.classList.remove('sidebar-open');
  openButton?.addEventListener('click', () => body.classList.add('sidebar-open'));
  closeButton?.addEventListener('click', closeSidebar);
  backdrop?.addEventListener('click', closeSidebar);
  document.addEventListener('keydown', event => {
    if (event.key === 'Escape') closeSidebar();
  });
})();
