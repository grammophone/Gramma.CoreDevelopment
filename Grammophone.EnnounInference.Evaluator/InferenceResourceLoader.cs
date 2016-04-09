using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Grammophone.EnnounInference;
using System.Threading.Tasks;
using Grammophone.Windows;

namespace Grammophone.EnnounInference.Evaluator
{
	class InferenceResourceLoader
	{
		private Window ownerWindow;

		private Dictionary<InferenceResourceProvider, InferenceResource> resourcesByProvidersDictionary;

		public InferenceResourceLoader(Window ownerWindow)
		{
			if (ownerWindow == null) throw new ArgumentNullException("ownerWindow");

			this.ownerWindow = ownerWindow;

			this.resourcesByProvidersDictionary = new Dictionary<InferenceResourceProvider, InferenceResource>();
		}

		public InferenceResource Load(InferenceResourceProvider provider)
		{
			if (provider == null) throw new ArgumentNullException("provider");

			InferenceResource inferenceResource;

			if (!resourcesByProvidersDictionary.TryGetValue(provider, out inferenceResource))
			{
				var task = new Task<InferenceResource>(() => provider.Load());

				var taskWindow = new TaskWindow("Loading inference resource", task);

				taskWindow.Owner = ownerWindow;

				taskWindow.ShowDialog();

				if (!task.IsFaulted && !task.IsCanceled)
				{
					inferenceResource = task.Result;

					resourcesByProvidersDictionary[provider] = inferenceResource;
				}
			}

			return inferenceResource;
		}
	}
}
