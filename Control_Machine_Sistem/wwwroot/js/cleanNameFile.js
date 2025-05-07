function getCleanFileName(url) {
    if (!url) return "";

    let fileName = url.split('/').pop();
    console.log("cleanNameFile.js cargado correctamente!");
    let match = fileName.match(/_(.+)$/);

    return match ? match[1] : fileName;

   
}
