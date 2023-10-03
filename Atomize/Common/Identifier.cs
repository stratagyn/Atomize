using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Atomize;

internal static class GlobalIdentifier
{
   private static readonly ObjectIDGenerator idGenerator;

   static GlobalIdentifier() { idGenerator = new ObjectIDGenerator(); }

   public static long GetId(object value) => 
      idGenerator.GetId(value, out var _);
}
