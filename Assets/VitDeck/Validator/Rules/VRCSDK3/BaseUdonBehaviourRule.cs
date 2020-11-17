#if VRC_SDK_VRCSDK3
using System;
using System.Collections.Generic;
using UnityEngine;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;
using VRC.Udon.Editor.ProgramSources.UdonGraphProgram;
using VRC.Udon.Graph;
using VRC.Udon.UAssembly.Disassembler;

namespace VitDeck.Validator
{
    /// <summary>
    /// UdonBehaviourを探してそれぞれバリデーションするためのクラス
    /// 詳細な規約はクラスを継承して ComponentLogic に記述する
    /// </summary>
    internal class BaseUdonBehaviourRule : ComponentBaseRule<UdonBehaviour>
    {
        public BaseUdonBehaviourRule(string name) : base(name)
        {
        }

        protected override void ComponentLogic(UdonBehaviour component)
        {
            // UdonProgramName
            var udonProgramName = component.programSource.name;
            Debug.Log(udonProgramName);

            // ノード一覧
            var graph = GetGraphData(component);
            if (graph != null)
            {
                List<string> nodeNames = new List<string>();
                foreach (var node in graph.nodes)
                {
                    nodeNames.Add(node.fullName);
                }
                Debug.Log(String.Join("\n", nodeNames));
            }

            // コード
            Debug.Log(GetDisassembleCode(component));
        }

        protected override void HasComponentObjectLogic(GameObject hasComponentObject)
        {
        }

        /// <summary>
        /// SerializedAssemblyからプログラムを取得
        /// </summary>
        /// <param name="component">VRC.Udon.UdonBehaviour</param>
        /// <returns>IUdonProgram</returns>
        protected static IUdonProgram GetUdonProgram(UdonBehaviour component)
        {
            return component.programSource.SerializedProgramAsset.RetrieveProgram();
        }

        /// <summary>
        /// プログラムからのディスアセンブル
        /// </summary>
        /// <param name="program">VRC.Udon.Common.Interfaces.IUdonProgram</param>
        /// <returns>string</returns>
        protected static string GetDisassembleCode(IUdonProgram program)
        {
            var disasm = new UAssemblyDisassembler();
            return String.Join("\n", disasm.DisassembleProgram(program));
        }

        /// <summary>
        /// バイトコードからのディスアセンブル
        /// </summary>
        /// <param name="component">VRC.Udon.UdonBehaviour</param>
        /// <returns>string</returns>
        protected static string GetDisassembleCode(UdonBehaviour component)
        {
            return GetDisassembleCode(GetUdonProgram(component));
        }

        /// <summary>
        /// UdonGraphの取り出し
        /// </summary>
        /// <param name="component">VRC.Udon.UdonBehaviour</param>
        /// <returns>VRC.Udon.Graph.UdonGraphData</returns>
        protected static UdonGraphData GetGraphData(UdonBehaviour component)
        {
            var programAsset = component.programSource as UdonGraphProgramAsset;
            return programAsset?.GetGraphData();
        }
    }
}
#endif
