(function () {
    'use strict';

    var nav = document.getElementById('nav');
    if (nav) {
        window.addEventListener('scroll', function () {
            nav.classList.toggle('scrolled', window.scrollY > 10);
        });
    }

    var toast = document.getElementById('toast');
    if (toast && toast.textContent && toast.textContent.trim()) {
        toast.style.display = 'block';
        setTimeout(function () { toast.style.display = 'none'; }, 4000);
    }

    var bars = document.querySelectorAll('[data-table-tools]');
    Array.prototype.forEach.call(bars, function (bar) {
        var table = document.querySelector(bar.getAttribute('data-target'));
        if (!table || !table.tBodies || table.tBodies.length === 0) return;

        var rows = Array.prototype.slice.call(table.tBodies[0].rows);
        var search = bar.querySelector('.tool-search');
        var sort = bar.querySelector('.tool-sort');
        var count = bar.querySelector('.tool-count');
        var reset = bar.querySelector('.tool-reset');
        if (!search || !sort || !count) return;

        function apply() {
            var q = (search.value || '').toLowerCase();
            var visible = 0;
            rows.forEach(function (r) {
                var ok = r.textContent.toLowerCase().indexOf(q) !== -1;
                r.style.display = ok ? '' : 'none';
                if (ok) visible++;
            });
            count.textContent = 'Показано ' + visible + ' из ' + rows.length;
        }

        search.addEventListener('input', apply);

        sort.addEventListener('change', function () {
            if (!sort.value) return;
            var parts = sort.value.split(':');
            var col = parseInt(parts[0], 10);
            var dir = parts[1];
            var tbody = table.tBodies[0];
            rows.sort(function (a, b) {
                var av = a.cells[col].getAttribute('data-sort') || a.cells[col].textContent;
                var bv = b.cells[col].getAttribute('data-sort') || b.cells[col].textContent;
                return av.localeCompare(bv, 'ru', { numeric: true }) * (dir === 'desc' ? -1 : 1);
            });
            rows.forEach(function (r) { tbody.appendChild(r); });
        });

        if (reset) {
            reset.addEventListener('click', function () {
                search.value = '';
                sort.value = '';
                apply();
            });
        }


        apply();
        /* ----- role-based UI: cosmetic only, server enforces the real rules ----- */
        (function () {
            'use strict';
            var body = document.body;
            var role = body.getAttribute('data-role') || '';
            var page = body.getAttribute('data-page') || '';
            var admin = role === 'Administrator';
            var patient = role === 'Patient';

            function hide(sel) {
                Array.prototype.forEach.call(document.querySelectorAll(sel), function (el) {
                    el.style.display = 'none';
                });
            }

            if (!admin) {
                hide('.btn-edit, .btn-del, .bulk-bar, #checkAll, .row-check, .nav-cta');
                hide('#drawerEdit, #drawerDelete');
                if (!(patient && page === 'Appointments')) hide('.btn-add');
            }
            if (patient) {
                hide('.status-card');
                hide('.nav a[href="/Patients"]');
            }
        })();
    });
})();