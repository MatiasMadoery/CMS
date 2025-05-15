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

window.printDocument = printDocument;
document.addEventListener("DOMContentLoaded", applyCleanFileNames);


