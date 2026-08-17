// Apply the saved dark theme before Angular starts to avoid a light-theme flash.

if (localStorage.getItem('theme') === 'dark') {
  document.documentElement.classList.add('app-dark');
}
