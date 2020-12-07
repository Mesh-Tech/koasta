function bindSelectInputs() {
    $("[data-input-type='select-input']").select2({
        width: '100%'
    })

    $("[data-input-type='select-image-input']").select2({
        width: '100%',
        dropdownCssClass: 'select--dropdown',
        templateResult: state => {
            const $el = $(state.element)
            const type = $el.data('opttype')
            if (type === 'none') {
                return 'None'
            }

            const imageUrl = $el.data('image-url')
            return $(`<div class="search-result__image-card">
                <img src="${imageUrl}" alt="${state.text}">
                <span>${state.text}</span>
            </div>`)
        }
    })
}

$().ready(() => {
    if (window.__controller) {
        window.__controller.abort()
    }
    window.__controller = new AbortController()

    bindSelectInputs()

    let listener
    listener = () => {
        window.__controller.abort()
        document.removeEventListener("turbolinks:before-render", listener)
        $("[data-input-type='tags-input']").each((_, el) => {
            $(el).data('tagify').destroy()
        })

        $("[data-input-type='select-input']").each((_, el) => {
            $(el).select2.destroy()
        })

        $("[data-input-type='select-image-input']").each((_, el) => {
            $(el).select2.destroy()
        })
    }

    document.addEventListener("turbolinks:before-render", listener)
})
