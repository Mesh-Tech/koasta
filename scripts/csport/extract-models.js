const fs = require('fs')
const path = require('path')
const Handlebars = require('handlebars')
const rawTemplate = fs.readFileSync("model.hbs", 'utf8')
const template = Handlebars.compile(rawTemplate, {noEscape: true})
const changeCase = require('change-case')

const modelsPath = path.join(__dirname, "..", "..", "services", "service-core", "src", "model")
const dtoModelsPath = path.join(__dirname, "..", "..", "services", "service-core", "src", "dtomodel")
const modelsDestination = path.join(__dirname, "..", "..", "services", "shared", "models")
const dtoModelsDestination = path.join(__dirname, "..", "..", "services", "shared", "dtomodels")

const modelsFiles = fs.readdirSync(modelsPath).filter(f => f.includes('.rs'))
const dtoModelsFiles = fs.readdirSync(dtoModelsPath).filter(f => f.includes('.rs'))

const mapping = {
    "bool": "bool",
    "char": "char",
    "i8": "sbyte",
    "i16": "short",
    "i32": "int",
    "i64": "long",
    "isize": "long",
    "u8": "byte",
    "u16": "ushort",
    "u32": "uint",
    "u64": "ulong",
    "usize": "ulong",
    "f32": "float",
    "f64": "double",
    "&str": "string",
    "String": "string",
    "Vec<i32>": "List<int>",
    "Vec<i16>": "List<short>",
    "Vec<i8>": "List<sbyte>",
    "Vec<u32>": "List<uint>",
    "Vec<u16>": "List<ushort>",
    "Vec<u8>": "List<byte>",
    "DateTime<Utc>": "DateTime",
    "NaiveTime": "DateTime",
    "BigDecimal": "decimal"
}

const mapRsToCsDataType = (dataType) => {
    if (!(dataType in mapping)) {
        if (dataType.startsWith("Vec<")) {
            return dataType.replace("Vec", "List")
        }

        console.error(`Missing dataType: ${dataType}`)
    }

    return mapping[dataType]
}

const objs = []

const convertAll = (models, source, destination, isDto = false) => {
    models.forEach(file => {
        let data = fs.readFileSync(path.join(source, file), 'utf8')
        let pos = data.indexOf("pub struct")
    
        if (pos === -1) {
            console.error(`Unable to find pub struct in file: ${file}`)
            return
        }
    
        data = data.substr(pos)
        pos = data.indexOf("}")
    
        if (pos === -1) {
            console.error(`Unable to find end of struct in file: ${file}`)
            return
        }
    
        data = data.substr(0, pos)
        let lines = data.split('\n')
        const className = lines[0].replace("pub struct", "").replace("{", "").trim()
    
        lines = lines.slice(1)
        const entries = lines.map(s => s.replace("pub ", "").trim()).map(s => {
            if (s === "") {
                return;
            }
    
            const components = s.split(":").map(c => c.replace(',', '').trim())
            if (components.length < 2) {
                return;
            }
            let nullable = false
            
            if (components[1].includes("Option<")) {
                nullable = true
                components[1] = components[1].substr(0, components[1].length - 1).replace("Option<", "")
            }
    
            return {
                dbFieldName: changeCase.camelCase(components[0]),
                fieldName: changeCase.upperCaseFirst(changeCase.camelCase(components[0])),
                dataType: mapRsToCsDataType(components[1]),
                nullable
            }
        }).filter(c => c)

        objs.push({ className, entries, isDto })
    })
}

convertAll(modelsFiles, modelsPath, modelsDestination)
convertAll(dtoModelsFiles, dtoModelsPath, dtoModelsDestination, true)
fs.writeFileSync(path.join(__dirname, 'models.json'), JSON.stringify(objs, null, 2), 'utf8')