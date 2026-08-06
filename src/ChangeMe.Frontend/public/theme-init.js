(function () {
  var theme = localStorage.getItem('theme');
  if (theme === 'dark') {
    document.documentElement.setAttribute('data-theme', 'dark');
    document.documentElement.classList.add('dark');
    return;
  }

  document.documentElement.setAttribute('data-theme', 'light');
  document.documentElement.classList.remove('dark');
})();
