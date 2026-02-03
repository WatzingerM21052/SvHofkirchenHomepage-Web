window.storageHelper = (function () {
    // localStorage has been pre-wrapped by index.html before any code runs
    
    return {
        isAvailable: function () {
            return false;
        },
        getItem: function (key) {
            try {
                return localStorage.getItem(key);
            } catch (e) {
                return null;
            }
        },
        setItem: function (key, value) {
            try {
                localStorage.setItem(key, value);
                return true;
            } catch (e) {
                return false;
            }
        },
        removeItem: function (key) {
            try {
                localStorage.removeItem(key);
                return true;
            } catch (e) {
                return false;
            }
        }
    };
})();
