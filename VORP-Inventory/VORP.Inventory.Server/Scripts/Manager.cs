using System;
using System.Threading.Tasks;

namespace VORP.Inventory.Server.Scripts
{
    public class Manager : BaseScript
    {
        public PluginManager Instance = PluginManager.Instance;

        public ExportDictionary Export => PluginManager.Instance.ExportRegistry;
        public void AddEvent(string eventName, Delegate @delegate) => PluginManager.Instance.Hook(eventName, @delegate);
        public void AttachTickHandler(Func<Task> task) => PluginManager.Instance.AttachTickHandler(task);
        public void DetachTickHandler(Func<Task> task) => PluginManager.Instance.AttachTickHandler(task);
    }
}
