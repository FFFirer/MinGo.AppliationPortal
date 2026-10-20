(function () {
    var deferredPrompt = null;
    var btn = document.getElementById('installBtn');
    var modal = document.getElementById('installModal');
    var guideBody = document.getElementById('installGuideBody');
    if (!btn || !modal) return;

    function isStandalone() {
        try {
            if (window.matchMedia('(display-mode: standalone)').matches) return true;
            if (window.matchMedia('(display-mode: fullscreen)').matches) return true;
            if (window.matchMedia('(display-mode: minimal-ui)').matches) return true;
            if (navigator.standalone === true) return true;
        } catch (e) { /* ignore */ }
        return false;
    }

    function hideBtn() {
        btn.classList.add('hidden');
        btn.setAttribute('aria-hidden', 'true');
    }

    function showBtn(title) {
        btn.classList.remove('hidden');
        btn.setAttribute('aria-hidden', 'false');
        if (title) {
            btn.setAttribute('title', title);
            btn.setAttribute('aria-label', title);
        }
    }

    function platform() {
        var ua = navigator.userAgent || '';
        if (/iPhone|iPad|iPod/i.test(ua)) return 'ios';
        if (/Android/i.test(ua)) return 'android';
        return 'desktop';
    }

    function origin() {
        return location.origin || (location.protocol + '//' + location.host);
    }

    function esc(s) {
        return String(s).replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
    }

    function buildGuide() {
        var secure = !!window.isSecureContext;
        var p = platform();
        var o = esc(origin());
        var rows = [];

        if (deferredPrompt) {
            rows.push('<li>当前浏览器已可安装：点顶栏「安装」，或地址栏/菜单中的「安装应用」。</li>');
        }

        if (secure && !deferredPrompt) {
            rows.push('<li>打开浏览器菜单 →「安装应用 / Install」，或地址栏右侧安装图标。</li>');
        }

        if (!secure) {
            rows.push(
                '<li><strong>HTTP 明文不能直接原生安装</strong>（浏览器安全策略）。可选：<br/>' +
                '① 在 Chrome 打开 <code class="text-xs">chrome://flags/#unsafely-treat-insecure-origin-as-secure</code>，' +
                '填入 <code class="text-xs">' + o + '</code>，Enabled 后重启浏览器，再回到本页点安装；<br/>' +
                '② 把服务端口转发到本机，用 <code class="text-xs">http://localhost:端口</code> 访问后再安装（localhost 是安全上下文）。</li>'
            );
        }

        if (p === 'ios') {
            rows.push('<li>iPhone / iPad：Safari 打开本站 →「分享」→「添加到主屏幕」。</li>');
        } else if (p === 'android') {
            rows.push('<li>Android Chrome：菜单 ⋮ →「安装应用」或「添加到主屏幕」。</li>');
        } else {
            rows.push('<li>桌面 Chrome / Edge：菜单 →「安装应用」；安装后可固定到任务栏。</li>');
        }

        if (!secure && (p === 'ios' || p === 'android')) {
            rows.push('<li>手机浏览器菜单里的「添加到主屏幕」在 HTTP 下仍可用，会生成桌面图标（非完整 PWA 安装）。</li>');
        }

        return '<ul class="space-y-3 text-sm text-gray-600 dark:text-gray-300 list-disc list-outside pl-5">' + rows.join('') + '</ul>';
    }

    function openModal() {
        if (guideBody) guideBody.innerHTML = buildGuide();
        modal.classList.remove('hidden');
        modal.setAttribute('aria-hidden', 'false');
        document.body.style.overflow = 'hidden';
    }

    function closeModal() {
        modal.classList.add('hidden');
        modal.setAttribute('aria-hidden', 'true');
        document.body.style.overflow = '';
    }

    window.addEventListener('beforeinstallprompt', function (e) {
        e.preventDefault();
        deferredPrompt = e;
        if (!isStandalone()) showBtn('安装应用');
    });

    window.addEventListener('appinstalled', function () {
        deferredPrompt = null;
        hideBtn();
    });

    btn.addEventListener('click', function () {
        if (isStandalone()) {
            hideBtn();
            return;
        }
        if (deferredPrompt) {
            var promptEvent = deferredPrompt;
            deferredPrompt = null;
            promptEvent.prompt().catch(function () { /* ignore */ });
            // Show button again if user dismissed without installing
            setTimeout(function () {
                if (!isStandalone() && !deferredPrompt) showBtn('安装 / 查看安装方式');
            }, 500);
            return;
        }
        openModal();
    });

    modal.querySelectorAll('[data-install-close]').forEach(function (el) {
        el.addEventListener('click', closeModal);
    });
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape' && !modal.classList.contains('hidden')) closeModal();
    });

    // Always expose an install/help entry outside standalone —
    // HTTP visitors otherwise never learn how to install.
    if (isStandalone()) {
        hideBtn();
    } else {
        showBtn('安装 / 查看安装方式');
    }
})();
