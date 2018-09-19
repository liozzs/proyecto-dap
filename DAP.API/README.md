#proyecto-dap

##API

- Login: **POST** api/login
  - Request:
    - Body: {Email: \_, Password: \_}
  - Response:
    - **Ok**({token: \_})
    - **Unauthorized**()
- Create User: **POST** api/login/create
  - Request:
    - Body: {Nombre: \_, Email: \_, Password: \_}
  - Response:
    - **Ok**({token: \_})
    - **BadRequest**()
- Get User Information: **GET** api/usuarios
  - Request:
    - Header: Authorization: Bearer _token_
  - Response:
    - **Ok**({id: \_, nombre: \_, password: \_, salt: \_, email: \_, dispensers:[{id: \_, direccionMAC: \_, nombre: \_, usuarios: null}, _otherDispensers_... ]})
    - **Unauthorized**()
    - **Not Found**()
- Get User Dispensers: **GET** api/usuarios/dispensers
    - Request:
      - Header: Authorization: Bearer _token_
    - Response:
      - **Ok**([{id: \_, direccionMAC: \_, nombre: \_, usuarios: null}, _otherDispensers_... ])
      - **Unauthorized**()
      - **Not Found**()
- Create Dispenser for Current User: **POST** api/usuarios/dispensers
  - Request:
    - Header: Authorization: Bearer _token_
    - Body: {DireccionMAC: \_, Nombre: \_}
  - Response:
    - **Ok**()
    - **BadRequest**()
- Send Message From DAP: **POST** api/dispensers/message
  - Request:
    - Body: {DireccionMAC: \_, Codigo: \_, Receptaculo: \_, Pastilla: \_, Horario: \_, CantidadRestante: \_}
  - Response:
    - **Ok**()
    - **NotFound**()

- Get All Users Information: **GET** api/usuarios/all
  - Response:
    - **Ok**([{id: \_, nombre: \_, password: \_, salt: \_, email: \_, dispensers:[{id: \_, direccionMAC: \_, nombre: \_, usuarios: null}, _otherDispensers_... ]}, _otherUsers_... ])
- Get All Dispensers Information: **GET** api/dispensers/all
  - Response:
    - **Ok**(
      [{id: \_, direccionMAC: \_, nombre: \_, usuarios: [{id: \_, nombre: \_, password: \_, salt: \_, email: \_, dispensers: null}, _otherUsers_... ]}, _otherDispensers_... ])
