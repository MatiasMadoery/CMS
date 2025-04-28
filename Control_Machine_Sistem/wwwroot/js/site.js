function openMenu() {
    var menu = document.getElementById("menuQuery");

    if (menu) {
        menu.classList.toggle("menuMobile-Visible");
        menu.classList.toggle("menuMobile-Invisible");
    }
}

$(document).ready(function () {
    $.fn.select2.defaults.set("debug", true); // Debug para ver errores

    $('.select2').each(function () {
        var url = $(this).data('url');
        console.log("URL: ", url); // Esto te muestra si la URL se está mandando bien

        $(this).select2({
            placeholder: "Buscar cliente...",
            allowClear: true,
            ajax: {
                url: url,
                dataType: 'json',
                delay: 250,
                data: function (params) {
                    return {
                        term: params.term
                    };
                },
                processResults: function (data) {
                    console.log("Resultados: ", data); // Ver los resultados en la consola
                    return {
                        results: data
                    };
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    console.error("Error en la petición: ", textStatus, errorThrown);
                },
                cache: true
            },
            minimumInputLength: 2,
            language: {
                inputTooShort: function () {
                    return "Escribe al menos 2 letras...";
                },
                searching: function () {
                    return "Buscando...";
                },
                noResults: function () {
                    return "No se encontraron resultados";
                }
            }
        });
    });
});

