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

document.addEventListener("DOMContentLoaded", applyCleanFileNames);