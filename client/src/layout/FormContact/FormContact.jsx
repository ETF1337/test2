import React, { useState, useEffect } from "react";
import * as signalR from "@microsoft/signalr";
import "./FormContact.css"; // Импорт стилей

const FormContact = (props) => {
    const [contactName, setContactName] = useState("");
    const [contactEmail, setContactEmail] = useState("");
    const [notification, setNotification] = useState(null);
    const [isLoading, setIsLoading] = useState(false);

    useEffect(() => {
        const connection = new signalR.HubConnectionBuilder()
            .withUrl("http://localhost:5000/downloadNotificationHub", {
                withCredentials: true,
            })
            .build();

        connection.on("ReceiveMessage", (message) => {
            console.log(message);
            setNotification("Подтвердите выгрузку в Excel");
        });

        connection.start()
            .then(() => console.log("Connected to SignalR hub"))
            .catch(err => console.error("Ошибка соединения с SignalR:", err.toString()));

        return () => {
            connection.stop();
        };
    }, []);

    const submit = () => {
        if (contactName === "" || contactEmail === "") return;
        props.addContact(contactName, contactEmail);
        setContactName("");
        setContactEmail("");
    };

    const exportToExcel = () => {
        setNotification("Подтвердите выгрузку в Excel");
    };

    const handleUserResponse = async (response) => {
        setNotification(null);
        if (response === 'accepted') {
            setIsLoading(true); 
            setTimeout(async () => {
                const response = await fetch('/api/ContactManagement/contacts/excel', {
                    method: 'GET',
                    headers: {
                        'Content-Type': 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
                    }
                });
    
                if (response.ok) {
                    const blob = await response.blob();
                    const url = window.URL.createObjectURL(blob);
                    const link = document.createElement('a');
                    link.href = url;
                    link.setAttribute('download', 'contacts.xlsx');
                    document.body.appendChild(link);
                    link.click();
                    link.remove();
                } else {
                    console.error('Ошибка при выгрузке файла:', response.statusText);
                }
                setIsLoading(false);
            }, 5000); 
    
            setTimeout(() => {
                setIsLoading(false); 
            }, 5000);
        } else {
            setNotification(null); 
            setIsLoading(false); 
        }
    };

    return (
        <div>
            <div className="mb-3">
                <form>
                    <div className="mb-3">
                        <label className="form-label">Введите имя:</label>
                        <input
                            className="form-control"
                            type="text"
                            value={contactName}
                            onChange={(e) => { setContactName(e.target.value) }}
                        />
                    </div>
                    <div className="mb-3">
                        <label className="form-label">Введите e-mail:</label>
                        <textarea
                            className="form-control"
                            value={contactEmail}
                            onChange={(e) => { setContactEmail(e.target.value) }}
                            rows={1}
                        />
                    </div>
                </form>
            </div>

            {notification && (
                <div className="notification-modal">
                    <div className="notification-content">
                        <p>{notification}</p>
                        <button onClick={() => handleUserResponse('accepted')} className="btn btn-success">Подтверждаю</button>
                        <button onClick={() => handleUserResponse('declined')} className="btn btn-danger">Отказываюсь</button>
                    </div>
                </div>
            )}

            {isLoading && (
                <div className="loading-modal">
                    <div className="loading-content">
                        <p>Ожидайте загрузку файла...</p>
                    </div>
                </div>
            )}

            <div className="button-container">
                <button className="btn btn-primary" onClick={submit}>
                    Добавить контакт
                </button>
                <button className="btn btn-secondary" onClick={exportToExcel}>
                    Выгрузить в Excel
                </button>
            </div>
        </div>
    );
}

export default FormContact;