const fs = require('fs')
const path = require('path')
const Handlebars = require('handlebars')
Handlebars.registerHelper('toJSON', function(object){
	return new Handlebars.SafeString(JSON.stringify(object));
});

const rawTemplate = fs.readFileSync("repository.hbs", 'utf8')
const template = Handlebars.compile(rawTemplate, {noEscape: true})
const changeCase = require('change-case')
const moment = require('moment')
const destination = path.join(__dirname, "..", "..", "services", "Shared", "Repository")

function generateIndefiniteArticle (resourceTitle) {
    const name = resourceTitle.toLowerCase()
    return name.endsWith('y') ? `${resourceTitle.slice(0, resourceTitle.length - 1)}ies` : `${resourceTitle}s`
}

const models = process.argv.length > 2
  ? require('./models.json').filter(m => m.className === process.argv[2])
  : require('./models.json')

models
    .filter(m => !m.isDto && m.className != 'CompanyAccountInfo' && m.className != 'MenuAvailability')
    .map(m => ({
        dataType: m.className,
        tableName: m.dbTableName,
        dataTypePlural: generateIndefiniteArticle(changeCase.upperCaseFirst(changeCase.camelCase(m.className))),
        resourceId: m.resourceId,
        currentDateTime: moment(new Date()).toLocaleString(),
        fields: m.entries
            .filter(e => e.dbFieldName)
            .filter(e => e.dbFieldName !== `${changeCase.camelCase(m.className)}Id`)
            .filter(e => !e.dataType.includes('<'))
    }))
    .forEach(f => {
        const outData = template(f)
        const p = path.join(destination, `${changeCase.pascalCase(f.dataType)}RepositoryGenerated.cs`)
        try {
            fs.unlinkSync(p)
        } catch (err) {}
        fs.writeFileSync(p, outData, 'utf8')
    })
