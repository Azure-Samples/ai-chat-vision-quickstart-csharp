(function(w) {

    const err = msg => () => console.error(`focusTrap - ${msg}`);

    w.focusTrap = {

        initialized: false,

        button: {
            click: err("no button captured."),
        },

        textArea: {
            focus: err("no textArea captured."),
            addEventListener: err("no textArea captured."),
        },

        init: (ta, btn) => {
            
            if (w.focusTrap.initialized) {
                return;
            }

            w.focusTrap.initialized = true;
            
            w.focusTrap.textArea = ta || this.textArea;
            w.focusTrap.button = btn || this.button;

            w.focusTrap.textArea.addEventListener('keydown', function(e) {
                if (e.key === 'Enter' || e.key === "NumpadEnter") {
                    e.preventDefault();
                    w.focusTrap.button.click();
                }             
            }); 
        },
    };

    w.init = (ta, btn) => focusTrap.init(ta, btn);       

})(window);