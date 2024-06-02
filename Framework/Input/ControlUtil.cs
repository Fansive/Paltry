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

    /*>>ʹ��ǰ��<<
     *  1.������Ϸ�������Paltry.MonoAgent,�������е�����ControlUtil.Init()(Ĭ���ѵ���)
     *  2.������Input System Package   
     *>>Workflow<<
     *  1.����InputActions�ļ�,�����б༭����ĸ��ְ������ֺ���,����Զ�����C#��
     *  2.newһ�����Զ�������Ķ���,��Ϊ����ʹ��
     *  3.ʹ��ʱ��ֱ��Enable�õ���,����ͨ��ControlUtil��SwitchActionMap,
     *      ������ȷ��ʼ��ֻ��һ��ActionMap����(�����Ҫͬʱ���������ֶ�����)
     *    
     *    
     *    
     *    
     *  */
    /// <summary>
    /// ���InputSystemʹ��,�ṩ�л�ActionMap�ͱ�ݸļ��ķ���
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
        /// �л�����һ��ActionMap,�÷����ڲ��ᱣ��ֻ��һ��ActionMap����
        /// </summary>
        public static void SwitchActionMap(InputActionMap actionMap)
        {
            activeActionMap?.Disable();
            (activeActionMap = actionMap).Enable();
        }

        /// <summary>
        /// �÷�����������Action�ļ���״̬,������ȷ������ǰ�ر�����Action�ļ��,�Ա������ð���ʱ���������¼�
        /// <para>����onComplete������������UI����Ϣ,��������ͨ��action.GetBindingDisplayString()��ȡ,���о����Ʋ�����,���ڵ���GetBindingDisplayStringʱ����DisplayStringOptions������</para>
        /// </summary>
        /// <param name="action">�ð�������InputAction</param>
        /// <param name="onComplete">���ڴ����ظ�����,string���ظ���Action��,�ɰ�������ҵ�������ĸ�������ͻ��</param>
        /// <param name="bindingIndex">�ð�����InputAction�е�����</param>
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
        /// ȡ���ϴε�Rebind����
        /// </summary>
        public static void CancelRebind()
        {
            rebindingInputAction?.ApplyBindingOverride(orgBinding);
            rebindingInputAction = null;
        }

        /// <summary>
        /// ��� <see cref="InputAction"/>��ĳ��Binding�Ƿ��������ڵ�
        /// <see cref="InputActionMap"/>������Binding�ظ�
        /// <para>����ֵΪ�ظ���action��(���ظ��򷵻�null)</para>
        /// </summary>
        /// <param name="targetBindingIndex">��Binding����action�е�����</param>
        /// <returns>�ظ���action��,���ظ�����null</returns>
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
        /// ������ҵİ����õ�<see cref="ControlBindingSavePath"/>
        /// </summary>
        public static void SaveBinding(this IInputActionCollection2 control)
        {
            string jsonStr = control.SaveBindingOverridesAsJson();
            File.WriteAllText(ControlBindingSavePath, jsonStr);
        }
        /// <summary>
        /// ��<see cref="ControlBindingSavePath"/>������ҵİ�����,���û���򲻼���(��ʹ��Ĭ�ϰ�������)
        /// �ɰ�Control��(�Զ���������)����������д,������Ϸ��ʼʱ����Init
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

