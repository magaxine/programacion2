﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Dominio
{
    public abstract class Envio :IComparable <Envio>
    {
        #region Atributos
        protected int nroEnvio;
        protected static int ultNroEnvio = 1; //necesito hacer una propiedad de esto? creo que lo uso solo internamente
        protected string nombreRecibio;
        protected string firmaRecibio; 
        protected string nombreDestinatario;
        protected Direccion dirDestinatario;
        protected List<EtapaEnvio> etapasDelEnvio;
        protected decimal precioFinal;
        protected float peso; // para paquetes se guarda en Kg, para documentos en Gramos (pero en ambos se ingresa en Kg).

        /// esto queda bien aca??? <---- todavia no lo puse en astah!!
        protected DateTime fechaIngreso;
        protected string fechaIngresoParaEntregar;
        /// ---
        
        #endregion
        
        #region Properties

        public int NroEnvio
        {
            get { return nroEnvio; }
            set { nroEnvio = value; } // si es un autonumerado, no deberia tener un set, no?
        }

        public string NombreRecibio
        {
            get { return nombreRecibio; }
            set { nombreRecibio = value; }
        }

        public string FirmaRecibio
        {
            get { return firmaRecibio; }
            set { firmaRecibio = value; }
        }

        public string NombreDestinatario
        {
            get { return nombreDestinatario; }
            set { nombreDestinatario = value; }
        }

        public Direccion DirDestinatario
        {
            get { return dirDestinatario; }
            set { dirDestinatario = value; }
        }

        public List<EtapaEnvio> EtapasDelEnvio
        {
            get { return etapasDelEnvio; }
            set { etapasDelEnvio = value; }
        }
        public decimal PrecioFinal
        {
            get { return precioFinal; }
            set { precioFinal = value; }
        }

        public float Peso
        {
            get { return peso; }
            set { peso = value; }
        }

        public DateTime FechaIngreso
        {
            get { return fechaIngreso; }
            set { fechaIngreso = value; }
        }

        // propiedad hecha para gridview de listado de envios
        public EtapaEnvio.Etapas Etapa
        {
            get { return this.ObtenerEtapaActual().Etapa; }
        }

        // propiedad hecha para gridview de envios atrasados5dias && para IComparable que ordena segun esto
        public string DocCliente
        {
            get { return Sistema.Instancia.BuscarClienteParaUnEnvio(this.NroEnvio); }
        }

        // para que devuelva en el gridview de envios el tipo de envio que es (pasado a string y despues de "Dominio.")
        public string TipoEnvio
        {
            get { return this.GetType().ToString().Split(new char[] { '.' })[1]; }
        }

        // property que esta hecha para las gridview de listar envios tambien, para visualizar el orden de la fecha de ingreso al estado 
        // paraEntregar. Tiene un atributo asociado. Lo transformo a string para poder dejar campo vacio si no llego a este estado (en vez del minimo de datetime)
        public string FechaIngresoParaEntregar
        {
            get { return fechaIngresoParaEntregar; }
        }

        // propiedad para las gridview. Porque siempre quierpo mostrar el peso en KG, aun para los documentos
        public float PesoEnKG
        {
            get
            {
                if (this.GetType() == typeof(EnvioDocumento))
                {
                    return this.peso / 1000;
                }
                else
                {
                    return this.peso;
                }
            }
        }


        #endregion

        #region Constructor

        // constructor convencional
        public Envio(string pNomDestinatario, Direccion pDirDestino, DateTime pFechaIngreso, OficinaPostal pOficinaIngreso) // <-- firmaRecibio: imagen!
        {
            this.NroEnvio = Envio.ultNroEnvio;
            Envio.ultNroEnvio += 1; // si pongo una propiedad en el atributo, cambiar aca <---
            this.NombreDestinatario = pNomDestinatario;
            this.DirDestinatario = pDirDestino;
            this.EtapasDelEnvio = new List<EtapaEnvio>();
            
            // si se crea un envio nuevo, en el constructor, de forma automática, se crea la primer 
            // etapa del envio, por eso este constructor toma como parametros tambien la fecha de ingreso
            // y la oficina en la que ingresó:
            EtapaEnvio unaEtapa = new EtapaEnvio(pFechaIngreso, EtapaEnvio.Etapas.EnOrigen, pOficinaIngreso);
           
            // agrego esa etapa en la lista de etapas recorridas de este envío
            this.EtapasDelEnvio.Add(unaEtapa);
            this.FechaIngreso = this.etapasDelEnvio[0].FechaIngreso;

        }

        // constructor para simulación
        public Envio()
        {

        }

        #endregion

        #region Comportamiento

        // forma de implementación depende de tipo de envío
        public abstract decimal CalcularPrecioFinal();

        // agregar etapas de rastreo al envío. Es polimórfico para EnvioDocumento, porque Documento necesita corroborar quien 
        // recibe el envio para agregarla (ver metodo en EnvioDocumento)
        public virtual bool AgregarEtapa(DateTime pFechaIngreso, EtapaEnvio.Etapas pEtapa, OficinaPostal pUbicacion, 
                                        string pFirmaRecibio, string pNombreRecibio, out string pMensajeError) 
        {
            string mensajeError = "";
            bool sePuedeAgregar = false;
            EtapaEnvio etapaActual = this.ObtenerEtapaActual();

            if (pFechaIngreso >= etapaActual.FechaIngreso)
            {
                // como EnOrigen se crea automaticamente con la creación del envío, no se puede utilizar nunca
                if (pEtapa != EtapaEnvio.Etapas.EnOrigen)
                {
                    // revisa secuencialidad
                    if (pEtapa >= etapaActual.Etapa)
                    {
                        // revisa si la etapa actual es igual que la anterior, porque solo voy a permitir esto para la etapa enTransito
                        if (etapaActual.Etapa == pEtapa)
                        {
                            // si la etapa actual es enTransito, se va a poder solo si estan en != oficinas
                            if (pEtapa == EtapaEnvio.Etapas.EnTransito)
                            {
                                // si la oficina es la misma que la anterior no deja ingresar la etapa (esto es para todos menos pEtapa = Entregado)
                                if (etapaActual.Ubicacion != pUbicacion)
                                {
                                    sePuedeAgregar = true;
                                }
                                else mensajeError = "La oficina postal no puede ser igual a la etapa anterior excepto para Entregado";
                            }
                            else mensajeError = "La etapa de envío no puede ser igual a la anterior a menos que el envío esté en tránsito";
                        }
                        // si la anterior y la que se pretende ingresar no son iguales (o sea, la nueva es mayor que la actual)
                        else
                        {
                            // si la etapa actual es Origen o Transito y es != de la nueva
                            if (etapaActual.Etapa == EtapaEnvio.Etapas.EnOrigen || etapaActual.Etapa == EtapaEnvio.Etapas.EnTransito)
                            {
                                // no pasar a Entregado desde una etapa diferente que no sea ParaEntregar
                                if (pEtapa == EtapaEnvio.Etapas.Entregado)
                                {
                                    mensajeError = "Aún no se puede entregar este envío porque se encuentra enOrigen/enTránsito";
                                }
                                // si quiero pasar a ParaEntregar, tengo que checkear que la direccion de la oficina final sea
                                // distinta que donde entró el emvio inicialmente
                                else if (pEtapa == EtapaEnvio.Etapas.ParaEntregar)
                                {
                                    if (pUbicacion != this.DevolverEtapaEnOrigen().Ubicacion)
                                    {
                                        sePuedeAgregar = true;
                                    }
                                    else mensajeError = "No se puede entregar un envío en la misma oficina que la de origen";
                                }
                                else
                                {
                                    if (pUbicacion != etapaActual.Ubicacion)
                                    {
                                        sePuedeAgregar = true;
                                    }
                                    else mensajeError = "La oficina postal no puede ser igual a la etapa anterior excepto para Entregado";
                                }
                            }
                            // si voy a agregar etapa Entregado (estando en ParaEntregar), tiene que ser en la misma oficina
                            else if (etapaActual.Etapa == EtapaEnvio.Etapas.ParaEntregar)
                            {
                                if (etapaActual.Ubicacion == pUbicacion)
                                {
                                    sePuedeAgregar = true;
                                }
                                else mensajeError = "El envío se debe entregar en la misma oficina donde se categorizó Para Entregar";
                            }
                        }
                    }
                    else mensajeError = "La etapa seleccionada ya no se puede ingresar para este envio";
                }
                else mensajeError = "La etapa seleccionada ya no se puede ingresar para este envio";
            }
            else mensajeError = "La fecha ingresada tiene que ser igual o posterior a la ingresada en la etapa previa";
            
            if (sePuedeAgregar)
            {
                EtapaEnvio etp = new EtapaEnvio(pFechaIngreso, pEtapa, pUbicacion);
                if (pEtapa == EtapaEnvio.Etapas.Entregado)
                {
                    this.nombreRecibio = pNombreRecibio;
                    this.firmaRecibio = pFirmaRecibio;
                }

                if (pEtapa == EtapaEnvio.Etapas.ParaEntregar)
                {
                    this.fechaIngresoParaEntregar = pFechaIngreso.ToString();
                }

                this.EtapasDelEnvio.Add(etp);
                mensajeError = "Se ha agregado la nueva etapa exitosamente!!";
            }

            pMensajeError = mensajeError;
            return sePuedeAgregar;
        }

        // Busca la EtapaEnvio que representa el ingreso a oficina Postal, y retorna la fecha en que se realizó
        // y se obtiene la diferencia entre el día actual y la fecha de ingreso.
        public int ObtenerDiasDesdeIngreso()
        {
            DateTime fechaIng = DateTime.Now;

            foreach (EtapaEnvio etp in this.EtapasDelEnvio)
            {
                if (etp.Etapa == EtapaEnvio.Etapas.EnOrigen) 
                {
                    fechaIng = etp.FechaIngreso;
                }
            }
            TimeSpan time = DateTime.Now - fechaIng;

            int dias = time.Days;

            return dias;
        }

        // revisa las fechas de ingreso de cada etapaDeEnvio, para quedarse con la etapa que tiene la fecha más cercana al 
        // día actual (la ultima fecha encontrada), y devolver el enum de Etapas en que se encuentra esa Etapa, que es el estado 
        // actual del envio <----
        public EtapaEnvio ObtenerEtapaActual()
        {
            DateTime ultimaFecha = DateTime.MinValue;
            EtapaEnvio etapaActual = null;

            if (this.etapasDelEnvio != null && this.etapasDelEnvio.Count > 0)
            {
                foreach (EtapaEnvio etp in this.etapasDelEnvio)
                {
                    if (etp.FechaIngreso >= ultimaFecha)
                    {
                        etapaActual = etp;
                        ultimaFecha = etapaActual.FechaIngreso;
                    }
                }
            }

            return etapaActual;
        }

        // devuelve la etapaEnvio que tiene el enum EnOrigen
        public EtapaEnvio DevolverEtapaEnOrigen ()
        {
            EtapaEnvio enOrigen = null;

            foreach (EtapaEnvio etapa in this.etapasDelEnvio)
            {
                if (etapa.Etapa == EtapaEnvio.Etapas.EnOrigen) enOrigen = etapa;
            }
            return enOrigen;
        }

        #endregion

        #region Implementacion IComparable
      
        //Metodo que me compara envios por fechas de manera descendente
        //No es lógico ordenar por fecha de entregado si el envío puede no haberse entregado aún 
        //(si está para entregar en la oficina final). Por lo tanto, ordénenlos por fecha de ENVIADO de forma ascendente.
        int IComparable<Envio>.CompareTo(Envio env)
        {
            int retorno = 0;

            Envio otro = env as Envio;

            if (otro != null)
            {
                // obtengo la etapa actual de los envios porque no se si el envio esta
                // "ParaEntregar" o "Entregado", ya que me sirven ambos. En el primer caso, tomo la ultima etapaDeEnvio, y en el 2do caso
                // tomo la penultima etapa.

                EtapaEnvio.Etapas etapaEnv1 = env.ObtenerEtapaActual().Etapa;
                EtapaEnvio.Etapas etapaOtroEnv2 = env.ObtenerEtapaActual().Etapa;
                int intRestarDeEnv1;
                int intRestarDeOtroEnv2;

                if (etapaEnv1 == EtapaEnvio.Etapas.ParaEntregar) intRestarDeEnv1 = 1;
                else intRestarDeEnv1 = 2;

                if (etapaOtroEnv2 == EtapaEnvio.Etapas.ParaEntregar) intRestarDeOtroEnv2 = 1;
                else intRestarDeOtroEnv2 = 2;

                // busco la ultima etapa del envio ("[XX.Count-1]") y busco la fecha de esa etapa, para compararla con la del otro envio
                retorno = this.EtapasDelEnvio[this.EtapasDelEnvio.Count - intRestarDeEnv1].FechaIngreso.
                        CompareTo(env.EtapasDelEnvio[env.EtapasDelEnvio.Count - intRestarDeOtroEnv2].FechaIngreso);                                        
            }

            return retorno;
        }

        #endregion
    }

}
