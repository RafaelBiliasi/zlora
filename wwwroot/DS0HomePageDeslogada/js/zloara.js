/* ============================================================
   ZLORA — Comportamentos da página pública
   Dependências: jQuery 3.2.1, Bootstrap Bundle, jQuery Mask
   ============================================================ */

(function ($) {
    'use strict';

    /* ========== ANO NO COPYRIGHT ========== */
    function atualizarAno() {
        var anoEl = document.getElementById('year');
        if (anoEl) anoEl.textContent = new Date().getFullYear();
    }

    /* ========== NAVBAR COM SCROLL ========== */
    function inicializarNavbarScroll() {
        var $nav = $('#nav-menu');
        if (!$nav.length) return;

        var ultimoEstado = false;
        function checar() {
            var scrolled = window.scrollY > 50;
            if (scrolled !== ultimoEstado) {
                $nav.toggleClass('is-scrolling', scrolled);
                ultimoEstado = scrolled;
            }
        }
        checar();
        window.addEventListener('scroll', checar, { passive: true });
    }

    /* ========== NAVEGAÇÃO INTERNA COM OFFSET DA NAVBAR ========== */
    function inicializarNavegacao() {
        $(document).on('click', 'a[href^="#"]', function (e) {
            var href = $(this).attr('href');
            if (!href || href === '#' || href.length < 2) return;

            var $target = $(href);
            if (!$target.length) return;

            e.preventDefault();
            var offset = $target.offset().top - 70;
            $('html, body').animate({ scrollTop: offset }, 500);

            // Fecha menu mobile, se aberto
            var $collapse = $('#menuPrincipal');
            if ($collapse.hasClass('show')) $collapse.collapse('hide');
        });
    }

    /* ========== ALTERNÂNCIA DE PLANOS ========== */
    function inicializarPlanos() {
        var $botoes = $('.price-switch .btn-price');
        var $grupos = $('.price-group');

        $botoes.on('click', function () {
            var id = this.id; // btn-anual | btn-semestral | btn-mensal
            var alvo = id.replace('btn-', ''); // anual | semestral | mensal

            $botoes.removeClass('active').attr('aria-pressed', 'false');
            $(this).addClass('active').attr('aria-pressed', 'true');

            $grupos.each(function () {
                var grupo = this.id.replace('group-', '');
                $(this).prop('hidden', grupo !== alvo);
            });
        });

        // Botões de contratação
        $('.js-plan').on('click', function () {
            var periodo = $(this).data('period');
            var faixa = $(this).data('range');

            // TODO: VALIDAR — direcionar para o fluxo real de contratação
            // Ex: window.location.href = '/checkout?periodo=' + periodo + '&faixa=' + faixa;
            console.info('[Zlora] Plano selecionado:', { periodo: periodo, faixa: faixa });

            // Enquanto não houver checkout, levamos o usuário ao cadastro
            var $cadastro = $('#cadastro');
            if ($cadastro.length) {
                $('html, body').animate({ scrollTop: $cadastro.offset().top - 70 }, 500);
            }
        });
    }

    /* Inicialização dos comportamentos originais da página pública. */
    $(function () {
        atualizarAno();
        inicializarNavbarScroll();
        inicializarNavegacao();
        inicializarPlanos();



    });

})(jQuery);