function addMoreFiles(containerId, inputName) {
    var container = document.getElementById(containerId);
    var input = document.createElement('input');
    input.type = 'file';
    input.name = inputName;
    input.className = 'form-control';
    container.appendChild(input);
    container.appendChild(document.createElement('br'));
}

function removeExistingFile(button, deletedFieldName) {
    var row = button.closest("tr");
    if (row) {
        var url = row.querySelector("a").getAttribute("href");

        var input = document.createElement("input");
        input.type = "hidden";
        input.name = deletedFieldName;
        input.value = url;

        document.getElementById("uploadForm").appendChild(input);
        row.remove();
    }
}

function getCleanFileName(url) {
    if (!url) return "";

    let fileName = url.split('/').pop();
    let match = fileName.match(/_(.+)$/);

    return match ? match[1] : fileName;
}

function applyCleanFileNames() {
    document.querySelectorAll(".manual-name, .spareKit-name, .document-name").forEach(function (element) {
        let link = element.tagName.toLowerCase() === "a" ? element : element.querySelector("a");
        if (link) {
            const url = link.getAttribute("href");
            if (url) {
                link.textContent = getCleanFileName(url);
            }
        }
    });
}    
      
function printDocument(url) {
    fetch(url)
        .then(response => {
            if (!response.ok) {
                throw new Error("Error al obtener el PDF");
            }
            return response.blob();
        })
        .then(blob => {
            const blobUrl = URL.createObjectURL(blob);
            const iframe = document.createElement("iframe");            
            iframe.style.position = "absolute";
            iframe.style.top = "-1000px";
            iframe.style.left = "-1000px";
            iframe.style.width = "0px";
            iframe.style.height = "0px";
            iframe.style.border = "none";
            iframe.src = blobUrl;
            document.body.appendChild(iframe);

            iframe.onload = function () {
                setTimeout(() => {
                    try {
                        iframe.contentWindow.focus();
                        iframe.contentWindow.print();
                    } catch (e) {
                        console.error("Error al llamar al print:", e);
                    }
                    document.body.removeChild(iframe);
                    URL.revokeObjectURL(blobUrl);
                }, 500);
            };
        })
        .catch(error => console.error("No se pudo obtener el PDF para imprimir:", error));
}

// ---(Pestañas y Filtros de Stock) ---

const StockManager = {
    // Maneja el cambio de pestañas y visibilidad de filtros
    switchTab: function (tabName) {
        const isMachines = tabName === 'machines';

        // Secciones de tablas
        const secMachines = document.getElementById('section-machines');
        const secAccessories = document.getElementById('section-accessories');
        if (secMachines) secMachines.style.display = isMachines ? 'block' : 'none';
        if (secAccessories) secAccessories.style.display = isMachines ? 'none' : 'block';

        // Botones de pestañas
        const btnMachines = document.getElementById('btn-machines');
        const btnAccessories = document.getElementById('btn-accessories');
        if (btnMachines) btnMachines.classList.toggle('active', isMachines);
        if (btnAccessories) btnAccessories.classList.toggle('active', !isMachines);

        // Filtros específicos de máquinas
        const colCat = document.getElementById('col-filter-category');
        const colMod = document.getElementById('col-filter-model');
        if (colCat) colCat.style.display = isMachines ? 'block' : 'none';
        if (colMod) colMod.style.display = isMachines ? 'block' : 'none';

        // Input oculto para persistencia
        const inputTab = document.getElementById('inputActiveTab');
        if (inputTab) inputTab.value = tabName;
    },

    // Lógica de cascada para Categoría -> Modelo
    initCategoryCascade: function (urlAction) {
        const categorySelect = document.getElementById('filterCategory');
        const modelSelect = document.getElementById('filterModel');

        if (categorySelect && modelSelect) {
            categorySelect.addEventListener('change', function () {
                const categoryId = this.value;
                modelSelect.innerHTML = '<option value="">-- Todos --</option>';

                if (categoryId) {
                    fetch(`${urlAction}?categoryId=${categoryId}`)
                        .then(response => response.json())
                        .then(data => {
                            data.forEach(m => {
                                const opt = document.createElement('option');
                                opt.value = m.id;
                                opt.text = m.name;
                                modelSelect.appendChild(opt);
                            });
                        });
                }
            });
        }
    }
};

// Exponer funciones globalmente
window.printDocument = printDocument;
window.switchTab = (name) => StockManager.switchTab(name);

// Inicialización
document.addEventListener("DOMContentLoaded", () => {
    applyCleanFileNames();

    // Si estamos en una vista con pestañas, inicializamos
    const inputTab = document.getElementById('inputActiveTab');
    if (inputTab) {
        StockManager.switchTab(inputTab.value);
    }
});


