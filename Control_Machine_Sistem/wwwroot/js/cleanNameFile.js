function getCleanFileName(url) {
    if (!url) return "";

    let fileName = url.split('/').pop();
    let match = fileName.match(/_(.+)$/);

    return match ? match[1] : fileName;
}
