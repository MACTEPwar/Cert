using Microsoft.ClearScript.V8;
using Microsoft.ClearScript;
using Microsoft.ClearScript.JavaScript;
using System.Xml.Linq;
using System;
using System.Text.RegularExpressions;

namespace TestWrapJS
{
    public class Test
    {
        public static string F1(string name)
        {
            var engine = new V8ScriptEngine();
            engine.Execute("function getGreeting(name) { return 'Hello, ' + name + '!'; }");
            var getGreeting = engine.Script.getGreeting;
            return (string)getGreeting(name);
        }

        public static void F2()
        {
            using (var engine = new V8ScriptEngine())
            {
                engine.AddHostType(typeof(Console));

                engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading;
                engine.DocumentSettings.SearchPath = @"C:\Users\macte\source\repos\altius_ui\node_modules\lodash\";

                Console.WriteLine("HELLO TEST");

                engine.Execute(new DocumentInfo() { Category = ModuleCategory.CommonJS }, @"
                    Console.WriteLine('Start Javascript');
                    
                    const lodash2 = require('lodash.js');


                    function getGreeting(name) { return name; }
                    let a = lodash2.upperCase('tttt');
                    Console.WriteLine(a);
                ");

                Console.WriteLine("END"); // "HELLO, WORLD"
            }
        }

        public static void F3()
        {
            using (var engine = new V8ScriptEngine(V8ScriptEngineFlags.EnableDebugging))
            {
                engine.AddHostType(typeof(Console));
                //engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading;
                //engine.DocumentSettings.SearchPath = @"C:\Users\macte\source\repos\testNpm\";
                //engine.Execute(new DocumentInfo() { Category = ModuleCategory.CommonJS }, @"
                //    Console.WriteLine('Start Javascript');
                //    var bn = require('node_modules/bn.js/lib/bn.js');
                //    var asn1 = require('node_modules/asn1.js/lib/asn1.js');
                //    var Box = require('node_modules/jkurwa/lib/index.js');
                //");
                //engine.Execute(new DocumentInfo() { Category = ModuleCategory.CommonJS }, @"load('node_modules/jkurwa/lib/index.js');");

                // Чтение кода главного файла библиотеки
                //string libraryCode = File.ReadAllText(@"C:\Users\macte\source\repos\testNpm\node_modules\jkurwa\lib\index.js");
                engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading;
                string libraryCode = File.ReadAllText(@"C:\Users\macte\source\repos\jkurwa\dist\bundle.js");

                // Загрузка кода библиотеки в интерпретатор
                engine.Execute(new DocumentInfo() { Category = ModuleCategory.CommonJS }, libraryCode);

                engine.Execute(@" var box = new Box({
                        keys: [{
                              privPath: `${__dirname}/../test/data/Key6929.cer`,
                              //password: '123',
                              //privPath: './FOP_key.dat',
                        }],
                        algo: gost89.compat.algos()
                    });
                ");



                // Получение объекта, который представляет экспортируемую функцию или класс из библиотеки
                //dynamic libraryObject = engine.Script.libraryObject;

                // Импорт всех зависимостей библиотеки
                //engine.AddHostObject("dependency1", new Dependency1());
                //engine.AddHostObject("dependency2", new Dependency2());
                // ...
            }
        }
    }
}