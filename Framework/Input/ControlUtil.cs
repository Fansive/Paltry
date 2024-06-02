using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Paltry;
using System;
using static UnityEngine.InputSystem.InputActionRebindingExtensions;
using System.IO;
using Paltry.Others;
namespace Paltry
{

    /*>>使用前提<<
     *  1.任意游戏物体挂载Paltry.MonoAgent,并在其中调用了ControlUtil.Init()(默认已调用)
     *  2.导入了Input System Package   
     *>>Workflow<<
     *  1.创建InputActions文件,在其中编辑所需的各种按键并分好组,最后自动生成C#类
     *  2.new一个该自动生成类的对象,作为单例使用
     *  3.使用时不直接Enable该单例,而是通过ControlUtil的SwitchActionMap,
     *      这样可确保始终只有一个ActionMap激活(如果需要同时激活多个可手动激活)
     *    
     *    
     *    
     *    
     *  */
    /// <summary>
    /// 结合InputSystem使用,提供切换ActionMap和便捷改键的方法
    /// </summary>
    public static class ControlUtil
    {
        private static InputAction rebindingInputAction;
        private static InputBinding orgBinding;
        private static InputActionMap activeActionMap;
        private static string ControlBindingSavePath => Application.persistentDataPath + "/" + ControlBindingSaveName + ".json";

        #region Configuration Data
        private static string ControlBindingSaveName => PaltryCongfig.SO.ControlBindingSaveName;
        #endregion
        public static void Init()
        {
            rebindingInputAction = null;
            activeActionMap = null;
        }
        /// <summary>
        /// 切换到另一套ActionMap,该方法内部会保持只有一个ActionMap激活
        /// </summary>
        public static void SwitchActionMap(InputActionMap actionMap)
        {
            activeActionMap?.Disable();
            (activeActionMap = actionMap).Enable();
        }

        /// <summary>
        /// 该方法不会设置Action的激活状态,所以请确保调用前关闭所有Action的检测,以避免设置按键时触发其他事件
        /// <para>可在onComplete中添加诸如更新UI的信息,按键名可通过action.GetBindingDisplayString()获取,若感觉名称不合适,可在调用GetBindingDisplayString时传入DisplayStringOptions来设置</para>
        /// </summary>
        /// <param name="action">该绑定所属的InputAction</param>
        /// <param name="onComplete">用于处理重复按键,string是重复的Action名,可帮助玩家找到具体和哪个按键冲突了</param>
        /// <param name="bindingIndex">该绑定在其InputAction中的索引</param>
        public static void Rebind(InputAction action,Action<string> onComplete=null,int bindingIndex=0)
        {
            rebindingInputAction = action;
            orgBinding = action.bindings[bindingIndex];

            action.PerformInteractiveRebinding(bindingIndex)
                .OnComplete(operation =>
            {
                onComplete?.Invoke(GetDuplicateAction(action,bindingIndex));
                operation.Dispose();
            }).Start();
        }
        /// <summary>
        /// 取消上次的Rebind操作
        /// </summary>
        public static void CancelRebind()
        {
            rebindingInputAction?.ApplyBindingOverride(orgBinding);
            rebindingInputAction = null;
        }

        /// <summary>
        /// 检测 <see cref="InputAction"/>中某个Binding是否与其所在的
        /// <see cref="InputActionMap"/>中其他Binding重复
        /// <para>返回值为重复的action名(不重复则返回null)</para>
        /// </summary>
        /// <param name="targetBindingIndex">该Binding在其action中的索引</param>
        /// <returns>重复的action名,不重复返回null</returns>
        public static string GetDuplicateAction(InputAction action, int targetBindingIndex)
        {
            InputActionMap context = action.actionMap;
            InputBinding targetBinding = action.bindings[targetBindingIndex];

            foreach (var binding in  context.bindings)
                if (binding.effectivePath == targetBinding.effectivePath && 
                    targetBinding.id != binding.id)
                    return binding.action;

            return null;
        }
        /// <summary>
        /// 保存玩家的绑定配置到<see cref="ControlBindingSavePath"/>
        /// </summary>
        public static void SaveBinding(this IInputActionCollection2 control)
        {
            string jsonStr = control.SaveBindingOverridesAsJson();
            File.WriteAllText(ControlBindingSavePath, jsonStr);
        }
        /// <summary>
        /// 从<see cref="ControlBindingSavePath"/>加载玩家的绑定配置,如果没有则不加载(就使用默认按键配置)
        /// 可把Control类(自定义生成类)像下面这样写,并在游戏开始时调用Init
        ///<code>
        ///<![CDATA[
        ///partial class Control
        /// {
        ///     public static Control Instance { get; private set; }
        ///     public static void Init()
        ///     {
        ///         Instance = new Control();
        ///         Instance.Load();
        ///     }
        /// }
        /// ]]>
        ///</code>
        /// </summary>
        public static void LoadBinding(this IInputActionCollection2 control)
        {
            if(File.Exists(ControlBindingSavePath))
                control.LoadBindingOverridesFromJson(File.ReadAllText(ControlBindingSavePath));
        }
    }

}

