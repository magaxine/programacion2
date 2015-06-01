﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio
{
    public class Usuario
    {
        #region Atributos

        private string user;
        private string password;
        private string nombre;
        private string apellido;
        private string documento;
        private string telefono;
        private Direccion direccionUsuario;
        private bool esAdmin;
        private List<Envio> enviosCliente;

        #endregion

        #region Properties

        public string User
        {
            get { return user; }
            set { user = value; }
        }

        public string Password
        {
            get { return password; }
            set { password = value; }
        }

        public string Nombre
        {
            get { return nombre; }
            set { nombre = value; }
        }

        public string Apellido
        {
            get { return apellido; }
            set { apellido = value; }
        }

        public string Documento
        {
            get { return documento; }
            set { documento = value; }
        }

        public string Telefono
        {
            get { return telefono; }
            set { telefono = value; }
        }

        public bool EsAdmin
        {
            get { return esAdmin; }
            set { esAdmin = value; }
        }

        public Direccion DireccionUsuario
        {
            get { return direccionUsuario; }
            set { direccionUsuario = value; }
        }

        public List<Envio> EnviosCliente
        {
            get { return enviosCliente; }
            set { enviosCliente = value; }
        }

        #endregion

        #region Constructor

        public Usuario(string pUser, string pPassword, string pNombre, string pApellido, string pDocumento,
            string pTelefono, Direccion direccion, bool esAdmin)
        {
            this.user = pUser;
            this.documento = pDocumento;
            this.password = pPassword;
            this.nombre = pNombre;
            this.apellido = pApellido;
            this.telefono = pTelefono;
            this.direccionUsuario = direccion;
            this.esAdmin = esAdmin;
        }

        #endregion

        #region Comportamiento

        /*Return lista de envios que superen el monto ingresado */
        public List<Envio> EnviosQueSuperanMonto(decimal pMonto)
        {
            List<Envio> lista = new List<Envio>();

            foreach (Envio env in enviosCliente)
            {
                if (env.PrecioFinal > pMonto)
                {
                    lista.Add(env);
                }
            }

            return lista;
        }

        /*Agrega un envio a la lista de envios que tiene el cliente */
        public void AgregarEnvio(Envio pEnvio)
        {
            if (enviosCliente == null)
            { enviosCliente = new List<Envio>(); }
            enviosCliente.Add(pEnvio);
        }

        /*Lista todos los envios del cliente que fueron entregados */
        public List<Envio> ListarEnviosEntregados()
        {
            List<Envio> lista = new List<Envio>();

            foreach (Envio env in enviosCliente)
            {
                if (EtapaEnvio.Etapas.Entregado == env.ObtenerEtapaActual().Etapa)
                {
                    lista.Add(env);
                }
            }

            return lista;
        }

        /*Devuelve el total facturado de ese cliente dado un intervalo*/

        public decimal TotalFacturadoEnIntervalo(DateTime pFechaInicio, DateTime pFechaFinal)
        {
            decimal total = 0M;

            foreach (Envio env in enviosCliente)
            {
                if (EtapaEnvio.Etapas.Entregado == env.ObtenerEtapaActual().Etapa && env.ObtenerEtapaActual().FechaIngreso >= pFechaInicio
                    && env.ObtenerEtapaActual().FechaIngreso <= pFechaFinal)
                {
                    total = total + env.PrecioFinal;
                }        
            }
            return total;
        }

        #endregion
    }
}





