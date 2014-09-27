using System;
using System.Security.Principal;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Security.Permissions;

namespace LinkCopyFTP
{
    class Program
    {
        private const string file_registry_path = @"HKEY_CLASSES_ROOT\*\shell\CopyLink";
        private const string settings_registrty_path = @"HKEY_CURRENT_USER\Software\CopyLink";

        public static bool IsAdmin()
        {
            var id = System.Security.Principal.WindowsIdentity.GetCurrent();
            var p = new System.Security.Principal.WindowsPrincipal(id);

            return p.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
        }

        public static void RunAsAdmin(string aFileName, string anArguments)
        {
            var processInfo = new System.Diagnostics.ProcessStartInfo();

            processInfo.FileName = aFileName;
            processInfo.Arguments = anArguments;
            processInfo.UseShellExecute = true;
            processInfo.Verb = "runas"; // здесь вся соль

            System.Diagnostics.Process.Start(processInfo);
        }

        static void SetRegValues()        
        {
            
        }

        [STAThread]
        static void Main(string[] args)
        {
            //AppDomain.CurrentDomain.SetPrincipalPolicy(PrincipalPolicy.WindowsPrincipal);

            if(args.Length == 0)
            {
                MessageBox.Show("Обнаружен запуск без параметров. Инструкция:" + Environment.NewLine + 
                                "Установка: LinkCopyFTP.exe /install <путь к папке public> <URL папки public в интернете>" + Environment.NewLine +
                                "Удаление: LinkCopyFTP.exe /uninstall" + Environment.NewLine +
                                "Получение ссылки: LinkCopyFTP.exe <путь к файлу для получения ссылки>","Справка LinkCopyFTP",MessageBoxButtons.OK,MessageBoxIcon.Information);
                return;
            }
            switch(args[0])
            {
                case "/install":
                    if (args.Length != 3)
                    {
                        MessageBox.Show("Неверный список параметров. Запустите без параметров для получения справки.", "Ошибка LinkCopyFTP", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    var app_path = Application.ExecutablePath;

                    if (IsAdmin())
                    {
                        var public_path = args[1];
                        var public_url = args[2];
                        
                        Registry.SetValue(file_registry_path, null, "Копировать ссылку");
                        Registry.SetValue(file_registry_path + "\\command", null, app_path + " \"%1\"");

                        Registry.SetValue(settings_registrty_path, "Public Path", public_path);
                        Registry.SetValue(settings_registrty_path, "Public URL", public_url);

                        MessageBox.Show("Успешно установлена", "Справка LinkCopyFTP", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        RunAsAdmin(app_path,args[0] + " \"" + args[1] + "\" \"" + args[2] + "\"");
                    }
                    
                    break;

                case "/uninstall":

                    
                    break;
                default:

                    var public_path_from_reg = (string)Registry.GetValue(settings_registrty_path, "Public Path", null);
                    var public_url_from_reg = (string)Registry.GetValue(settings_registrty_path, "Public URL", null);

                    if(public_path_from_reg == null || public_url_from_reg == null)
                    {
                        MessageBox.Show("Не найдены настройки. Необходимо запустить программу с ключом /install", "Ошибка LinkCopyFTP", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    if (args[0].ToLower().StartsWith(public_path_from_reg.ToLower()))
                    {
                        Clipboard.SetText(public_url_from_reg + args[0].Substring(public_path_from_reg.Length).Replace("\\", "/"));
                    }
                    else
                    {
                        Clipboard.SetText("Ошибка копирования ссылки");
                    }
                    break;

            }




            
            
        }
    }
}
