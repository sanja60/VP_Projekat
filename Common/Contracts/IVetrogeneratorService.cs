using System.ServiceModel;
using Common.Models;
using Common.Faults;

namespace Common.Contracts
{
    [ServiceContract]
    public interface IVetrogeneratorService
    {
        
        /// <param name="meta">Meta-informacije o sesiji (TurbineId, Operator, Description...)</param>
        [OperationContract]
        [FaultContract(typeof(ValidationFault))]
        void StartSession(SessionMeta meta);

        
        /// <param name="sample">Jedan red CSV-a u obliku WindTurbineSample klase.</param>
        [OperationContract]
        [FaultContract(typeof(DataFormatFault))]
        [FaultContract(typeof(ValidationFault))]
        void PushSample(WindTurbineSample sample);

        
        /// <param name="turbineId">ID vetroturbine čiji je prenos završen.</param>
        [OperationContract]
        void EndSession(string turbineId);
    }
}
