using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace DbHelper
{
    public class EntityBuilder<Entity>
    {
        private static readonly MethodInfo getValueMethod = typeof(IDataRecord).GetMethod("get_Item", new Type[] { typeof(int) });
        private static readonly MethodInfo isDBNullMethod = typeof(IDataRecord).GetMethod("IsDBNull", new Type[] { typeof(int) });
        private delegate Entity Load(IDataRecord dataRecord);

        private Load handler;
        private EntityBuilder() { }

        public Entity Build(IDataRecord dataRecord) { return handler(dataRecord); }

        public static EntityBuilder<Entity> CreateBuilder(IDataRecord dataRecord)
        {
            EntityBuilder<Entity> dynamicBuilder = new EntityBuilder<Entity>();
            DynamicMethod method = new DynamicMethod("IDataReaderDynamicCreateEntity", typeof(Entity), new Type[] { typeof(IDataRecord) }, typeof(Entity), true);
            ILGenerator generator = method.GetILGenerator();
            LocalBuilder result = generator.DeclareLocal(typeof(Entity));
            generator.Emit(OpCodes.Newobj, typeof(Entity).GetConstructor(Type.EmptyTypes));
            generator.Emit(OpCodes.Stloc, result);

            var properties = typeof(Entity).GetProperties();
            for (int i = 0; i < dataRecord.FieldCount; i++)
            {
                var propertie = properties.FirstOrDefault(x => x.Name.ToUpper().Equals(dataRecord.GetName(i).ToUpper()));
                if (propertie != null)
                {
                    PropertyInfo propertyInfo = typeof(Entity).GetProperty(propertie?.Name);
                    Label endIfLabel = generator.DefineLabel();
                    if (propertyInfo != null && propertyInfo.GetSetMethod() != null)
                    {
                        generator.Emit(OpCodes.Ldarg_0);
                        generator.Emit(OpCodes.Ldc_I4, i);
                        generator.Emit(OpCodes.Callvirt, isDBNullMethod);
                        generator.Emit(OpCodes.Brtrue, endIfLabel);
                        generator.Emit(OpCodes.Ldloc, result);
                        generator.Emit(OpCodes.Ldarg_0);
                        generator.Emit(OpCodes.Ldc_I4, i);
                        generator.Emit(OpCodes.Callvirt, getValueMethod);
                        generator.Emit(OpCodes.Unbox_Any, propertyInfo.PropertyType);
                        generator.Emit(OpCodes.Callvirt, propertyInfo.GetSetMethod());
                        generator.MarkLabel(endIfLabel);
                    }
                }
            }
            generator.Emit(OpCodes.Ldloc, result);
            generator.Emit(OpCodes.Ret);
            dynamicBuilder.handler = (Load)method.CreateDelegate(typeof(Load));
            return dynamicBuilder;
        }
    }

}
