using dnlib.DotNet;
using String_Encryption;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Trinity.Obfuscator_Engine
{
    class Engine
    {
        public static void InjectClass(ModuleDef module)
        {
            //We declare our Module, here we want to load the EncryptionHelper class
            ModuleDefMD typeModule = ModuleDefMD.Load(typeof(EncryptionHelper).Module);
            //We declare EncryptionHelper as a TypeDef using it's Metadata token (needed)
            TypeDef typeDef = typeModule.ResolveTypeDef(MDToken.ToRID(typeof(EncryptionHelper).MetadataToken));
            //We use confuserEX InjectHelper class to inject EncryptionHelper class into our target, under <Module>
            IEnumerable<IDnlibDef> members = InjectHelper.Inject(typeDef, module.GlobalType, module);
            //We find the Decrypt() Method in EncryptionHelper we just injected
            Trinity_Improved.Main.init = (MethodDef)members.Single(method => method.Name == "Decrypt");
            //we will call this method later

            //We just have to remove .ctor method because otherwise it will
            //lead to Global constructor error (e.g [MD]: Error: Global item (field,method) must be Static. [token:0x06000002] / [MD]: Error: Global constructor. [token:0x06000002] )
            foreach (MethodDef md in module.GlobalType.Methods)
            {
                if (md.Name == ".ctor")
                {
                    module.GlobalType.Remove(md);
                    //Now we go out of this mess
                    break;
                }
            }
        }

        public static int LoopCount { get; set; }
        private static Random random = new Random();
        public static string RandomString()
        {
            var rnd = new Random(DateTime.Now.Millisecond);
            int length = rnd.Next(20, 100);
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890#$^*%/迪勒艾艾弗杰伊诶吉伊尺屁娜哦娜诶吉伊艾艾艾弗娜开诶艾儿艾艾艾#";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public static string Encrypt(string clearText)
        {
            string Converted = "Æ" + Convert.ToBase64String(UTF8Encoding.UTF8.GetBytes(clearText)) + "-ProtectedByTrinity-" + "." + RandomString();
            return Converted;
        }
    }
}
