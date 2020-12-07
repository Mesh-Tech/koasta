const fs = require('fs')
const path = require('path')
const Handlebars = require('handlebars')
Handlebars.registerHelper('nullablePrimitive', function(object){
    return object.nullable && ['int', 'short', 'long', 'float', 'double', 'decimal', 'sbyte', 'byte', 'ushort', 'ulong', 'DateTimeOffset', 'TimeSpan'].includes(object.dataType)
        ? `?`
        : ``
});

const rawTemplate = fs.readFileSync("model.hbs", 'utf8')
const rawPatchTemplate = fs.readFileSync("patch-model.hbs", 'utf8')
const template = Handlebars.compile(rawTemplate, {noEscape: true})
const patchTemplate = Handlebars.compile(rawPatchTemplate, {noEscape: true})
const changeCase = require('change-case')

const modelsDestination = path.join(__dirname, "..", "..", "services", "Shared", "Models")
const dtoModelsDestination = path.join(__dirname, "..", "..", "services", "Shared", "Dtomodels")
const patchModelsDestination = path.join(__dirname, "..", "..", "services", "Shared", "Patchmodels")

const objs = require('./models.json')

objs.forEach(obj => {
    const outData = template(obj)
    const p = path.join(obj.isDto ? dtoModelsDestination : modelsDestination, `${changeCase.pascalCase(obj.className)}Generated.cs`)
    try {
        fs.unlinkSync(p)
    } catch (err) {}
    fs.writeFileSync(p, outData, 'utf8')

    if (!obj.isDto) {
        const patchObj = JSON.parse(JSON.stringify(obj))
        patchObj.entries = patchObj.entries.filter(e => !e.dataType.includes("<"))
        const outData = patchTemplate(patchObj)
        const p2 = path.join(patchModelsDestination, `${changeCase.pascalCase(obj.className)}PatchGenerated.cs`)
        try {
            fs.unlinkSync(p2)
        } catch (err) {}
        fs.writeFileSync(p2, outData, 'utf8')
    }
})
