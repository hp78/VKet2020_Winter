using System;
using UnityEngine;
using VitDeck.Exporter;
using VitDeck.Validator;
using VitDeck.Language;

namespace VitDeck.Main.ValidatedExporter
{
    public class ValidatedExporter
    {
        public static ValidatedExportResult ValidatedExport(string baseFolderPath, ExportSetting setting, bool forceExport = false, bool forceOpenScene = false)
        {
            if (setting == null)
                throw new ArgumentNullException("Argument `setting` is null.");
            if (baseFolderPath == null)
                throw new ArgumentNullException("Argument `baseFolderPath` is null.");
            var exportFolderPath = setting.ExportFolderPath;
            if (string.IsNullOrEmpty(exportFolderPath) || !exportFolderPath.StartsWith("Assets"))
                throw new ArgumentNullException("Invalid export folder path:" + exportFolderPath);
            ExportSettingStock settingStock = new ExportSettingStock(setting);
            var result = new ValidatedExportResult(forceExport);
            //validate
            if (!forceExport)
            {
                if (!string.IsNullOrEmpty(setting.ruleSetName))
                {
                    var ruleSet = GetRuleSet(setting.ruleSetName);
                    if (ruleSet == null)
                    {
                        result.log += LocalizedMessage.Get("ValidatedExporter.ProblemOccurredWhileValidating") +
                            LocalizedMessage.Get("ValidatedExporter.RuleNotFound", setting.ruleSetName);
                        return result;
                    }
                    try
                    {
                        result.validationResults = Validator.Validator.Validate(ruleSet, baseFolderPath, forceOpenScene);
                    }
                    catch (FatalValidationErrorException e)
                    {
                        result.log += LocalizedMessage.Get("ValidatedExporter.ProblemOccurredWhileValidating")
                            + System.Environment.NewLine + e.Message;
                        return result;
                    }
                    if (!result.HasValidationIssues(setting.ignoreValidationWarning ? IssueLevel.Error : IssueLevel.Warning))
                    {
                        result.log += LocalizedMessage.Get("ValidatedExporter.IssueFound") + System.Environment.NewLine;
                        return result;
                    }
                }
                else
                {
                    result.log += LocalizedMessage.Get("ValidatedExporter.SkipValidation") + System.Environment.NewLine;
                }
            }

            //export
            result.exportResult = Exporter.Exporter.Export(baseFolderPath, settingStock.GetSetting(), forceExport);
            if (!result.IsExportSuccess)
            {
                result.log += LocalizedMessage.Get("ValidatedExporter.Failed") + System.Environment.NewLine;
            }
            return result;
        }

        private static IRuleSet GetRuleSet(string ruleSetName)
        {
            return Validator.Validator.GetRuleSet(ruleSetName);
        }

        private class ExportSettingStock
        {
            public string SettingName = "";
            public string Description = "";
            public string ExportFolderPath = "Assets/Exports";
            public string fileNameFormat = "export_file.unitypackage";
            public string ruleSetName = "";
            public bool ignoreValidationWarning = false;
            private ExportSetting setting;

            public ExportSettingStock(ExportSetting setting)
            {
                this.setting = setting;
                this.SettingName = setting.SettingName;
                this.Description = setting.Description;
                this.ExportFolderPath = setting.ExportFolderPath;
                this.fileNameFormat = setting.fileNameFormat;
                this.ruleSetName = setting.ruleSetName;
                this.ignoreValidationWarning = setting.ignoreValidationWarning;
            }

            internal ExportSetting GetSetting()
            {
                if (setting != null)
                    return setting;
                else
                {
                    var newSetting = ScriptableObject.CreateInstance(typeof(ExportSetting)) as ExportSetting;
                    newSetting.SettingName = this.SettingName;
                    newSetting.Description = this.Description;
                    newSetting.ExportFolderPath = this.ExportFolderPath;
                    newSetting.fileNameFormat = this.fileNameFormat;
                    newSetting.ruleSetName = this.ruleSetName;
                    newSetting.ignoreValidationWarning = this.ignoreValidationWarning;
                    return newSetting;
                }
            }
        }
    }
}
