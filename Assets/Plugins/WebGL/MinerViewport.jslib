mergeInto(LibraryManager.library, {
    MinerCanvasCssHeight: function () {
        return Module.canvas ? Module.canvas.getBoundingClientRect().height : 0;
    }
});
