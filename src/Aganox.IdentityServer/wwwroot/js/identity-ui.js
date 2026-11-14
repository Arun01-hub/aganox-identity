(function () {
  const navToggle = document.querySelector('[data-nav-toggle]');
  const nav = document.querySelector('[data-shell-nav]');
  if (navToggle && nav) {
    navToggle.addEventListener('click', function () {
      const open = nav.classList.toggle('is-open');
      navToggle.setAttribute('aria-expanded', open ? 'true' : 'false');
    });
  }

  document.querySelectorAll('[data-password-toggle]').forEach(function (button) {
    button.addEventListener('click', function () {
      const inputId = button.getAttribute('aria-controls');
      const input = inputId ? document.getElementById(inputId) : null;
      if (!input) {
        return;
      }

      const show = input.type === 'password';
      input.type = show ? 'text' : 'password';
      button.textContent = show ? 'Hide' : 'Show';
      button.setAttribute('aria-pressed', show ? 'true' : 'false');
    });
  });

  document.querySelectorAll('[data-copy]').forEach(function (button) {
    button.addEventListener('click', async function () {
      const targetId = button.getAttribute('data-copy');
      const el = targetId ? document.getElementById(targetId) : null;
      const text = el ? (el.textContent || '').trim() : button.getAttribute('data-copy-text');
      if (!text) {
        return;
      }

      try {
        await navigator.clipboard.writeText(text);
        const label = button.getAttribute('data-copy-label') || 'Copy';
        button.textContent = 'Copied';
        setTimeout(function () {
          button.textContent = label;
        }, 2000);
      } catch {
        button.textContent = 'Select manually';
      }
    });
  });
})();
