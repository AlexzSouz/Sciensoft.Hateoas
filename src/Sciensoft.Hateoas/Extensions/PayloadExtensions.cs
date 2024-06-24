using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Sciensoft.Hateoas.Extensions
{
	internal static class PayloadExtensions
	{
		internal static object ToFinalPayload(this object originalModel, IList<object> links)
		{
			if (originalModel == null)
			{
				throw new InvalidOperationException("It must be a non-nullable instance.");
			}

			if (!links.Any())
			{
				return originalModel;
			}

			var originalType = originalModel.GetType();

			AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("Sciensoft.Hateoas.Links"), AssemblyBuilderAccess.RunAndCollect);
			ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("DynamicLinkModule");
			TypeBuilder typeBuilder = moduleBuilder.DefineType("LinkModel", TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AutoClass, null); // itemType

			var originalValues = new Dictionary<string, object>();

			if (originalModel is IEnumerable<object> collection)
			{
				//create a embedded property with the collection type and set all items into the embedded property
				CreateProperty(typeBuilder, "Embedded", collection.GetType());
				originalValues.TryAdd("Embedded", collection);
			}
			else
			{
				//create a property with the original type and set the original model into the Data property
				CreateProperty(typeBuilder, "Data", originalType);
				originalValues.TryAdd("Data", originalModel);
			}


			CreateProperty(typeBuilder, "Links", links.GetType());

			var payloadType = typeBuilder.CreateType();
			var payloadInstance = Activator.CreateInstance(payloadType);

			foreach (var pv in originalValues)
			{
				payloadType.GetProperty(pv.Key).SetValue(payloadInstance, pv.Value);
			}

			payloadType.GetProperty("Links").SetValue(payloadInstance, links);

			return payloadInstance;
		}

		private static void CreateProperty(TypeBuilder typeBuilder, string propertyName, Type propertyType)
		{
			FieldBuilder fieldBuilder = typeBuilder.DefineField("_" + propertyName, propertyType, FieldAttributes.Private);

			PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);
			MethodBuilder getPropMthdBldr = typeBuilder.DefineMethod("get_" + propertyName, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, propertyType, Type.EmptyTypes);
			ILGenerator getIl = getPropMthdBldr.GetILGenerator();

			getIl.Emit(OpCodes.Ldarg_0);
			getIl.Emit(OpCodes.Ldfld, fieldBuilder);
			getIl.Emit(OpCodes.Ret);

			MethodBuilder setPropMthdBldr =
				typeBuilder.DefineMethod("set_" + propertyName,
				  MethodAttributes.Public |
				  MethodAttributes.SpecialName |
				  MethodAttributes.HideBySig,
				  null, new[] { propertyType });

			ILGenerator setIl = setPropMthdBldr.GetILGenerator();
			Label modifyProperty = setIl.DefineLabel();
			Label exitSet = setIl.DefineLabel();

			setIl.MarkLabel(modifyProperty);
			setIl.Emit(OpCodes.Ldarg_0);
			setIl.Emit(OpCodes.Ldarg_1);
			setIl.Emit(OpCodes.Stfld, fieldBuilder);

			setIl.Emit(OpCodes.Nop);
			setIl.MarkLabel(exitSet);
			setIl.Emit(OpCodes.Ret);

			propertyBuilder.SetGetMethod(getPropMthdBldr);
			propertyBuilder.SetSetMethod(setPropMthdBldr);
		}
	}
}
