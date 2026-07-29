window.imageViewer = {
    enablePanZoom: function () {
        const img = document.getElementById("zoomImage");
        if (!img) return;

        const panzoomInstance = Panzoom(img, {
            maxScale: 5,
            minScale: 1,
            contain: "outside"
        });

        // Enable zoom with mouse wheel
        img.parentElement.addEventListener("wheel", panzoomInstance.zoomWithWheel);
    }
};