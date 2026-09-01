# BAIM — Product Context

## 1. Product overview

BAIM — Biznesin Avtomatlaşdırma və İnkişaf Mərkəzi.

BAIM — это web-приложение для управления проектами, задачами и командной работой.

По концепции продукт находится в одной категории с:

- Jira
- Azure DevOps
- Bitrix24

Но BAIM не должен быть визуальной копией ни одного из этих продуктов.

Мы можем использовать Jira, Azure DevOps, Bitrix24 и другие mature B2B/SaaS продукты как источник UX-паттернов и идей, но конечный интерфейс должен быть самостоятельным, последовательным и соответствовать существующему дизайну BAIM.

---

# 2. Главная бизнес-проблема

BAIM создаётся не только как внутренний task management tool.

Очень важная особенность продукта заключается в том, что клиентам компании также предоставляется доступ к системе.

Причина этого появилась из реальной бизнес-проблемы:

Клиенты компании постоянно спрашивают:

- На каком этапе находится мой проект?
- Что уже сделано?
- Что сейчас разрабатывается?
- Что осталось сделать?
- Есть ли задержки?
- Какие задачи завершены?
- Кто работает над проектом?
- Какой сейчас статус проекта?

Сотрудникам компании приходится постоянно вручную отвечать на эти вопросы.

BAIM должен уменьшить необходимость такого общения.

Вместо постоянных запросов клиент получает аккаунт и может самостоятельно открыть свой проект и увидеть разрешённую ему информацию.

Поэтому BAIM одновременно является:

1. Internal project management system.
2. Task tracking system.
3. Client transparency portal.
4. Collaboration system между компанией и её клиентами.

Это критически важно учитывать при проектировании UX.

Интерфейс нельзя проектировать только для технически опытных сотрудников.

Некоторые пользователи будут обычными клиентами, которые могут впервые видеть task management систему.

Поэтому приложение должно быть:

- intuitive;
- user-friendly;
- predictable;
- visually clear;
- understandable without training;
- consistent across the whole product.

---

# 3. Product quality expectations

BAIM — коммерческий продукт.

Он не должен производить впечатление:

- pet project;
- admin template;
- junior frontend project;
- набора случайных UI-компонентов;
- AI-generated interface;
- dashboard template;
- проекта, собранного из несвязанных между собой компонентов.

Пользователь или клиент должен чувствовать, что продукт:

- спроектирован профессиональной product team;
- имеет продуманную information architecture;
- имеет coherent design language;
- масштабируется;
- был разработан специально под бизнес;
- имеет зрелый UX.

Интерфейс должен выглядеть профессионально, современно и дорого, но не декоративно ради декоративности.

Premium UI не означает:

- больше gradients;
- больше shadows;
- больше карточек;
- больше цветов;
- больше иконок;
- больше декоративных элементов.

Premium UI означает:

- правильную hierarchy;
- typography;
- spacing;
- alignment;
- proportion;
- density;
- consistency;
- predictable interactions;
- attention to details;
- хорошо продуманную информационную архитектуру.

---

# 4. Core principles

При проектировании BAIM необходимо всегда соблюдать следующие правила.

## 4.1 No meaningless UI

Не добавлять информацию или UI-компоненты только для заполнения пространства.

Не создавать:

- decorative cards без смысловой причины;
- random statistics;
- fake dashboards;
- meaningless icons;
- unnecessary badges;
- repeated information;
- artificial sections только потому, что осталось свободное место.

Каждый UI-элемент должен иметь понятную функцию.

---

## 4.2 No excessive empty space

С другой стороны, интерфейс не должен выглядеть пустым.

Свободное пространство используется для:

- visual hierarchy;
- separation;
- readability;
- grouping.

Но данные должны быть правильно распределены по странице.

Информация должна делиться на:

### Primary
То, что пользователь должен увидеть первым.

### Important
Важная информация, которая должна быть легко заметна.

### Secondary
Дополнительная информация, которая нужна пользователю, но не должна конкурировать с primary content.

### Contextual
Информация, которую можно показывать по необходимости.

Необходимо избегать ситуации, когда всё имеет одинаковый визуальный вес.

---

# 5. Users and global roles

В BAIM существует четыре основных user roles:

1. Super Admin
2. Employee
3. Client Manager
4. Client

---

# 6. Super Admin

Super Admin имеет административные возможности системы.

Он может:

- регистрировать пользователей;
- создавать организации;
- выбирать организацию пользователя;
- создавать проекты;
- видеть все проекты;
- управлять участниками проектов;
- назначать роли внутри проектов.

Super Admin имеет доступ ко всем проектам.

---

# 7. Employee

Employee — пользователь, работающий на компанию, которая использует BAIM.

Название Employee используется как глобальная роль.

Это не обязательно support employee.

Employee может:

- работать с проектами;
- работать с tickets;
- работать с tasks;
- работать с subtasks;
- менять статусы;
- описывать выполненную работу;
- отмечать время;
- участвовать в проекте в различных project roles.

При регистрации Employee не требуется создавать или выбирать клиентскую организацию.

Для создания Employee достаточно основных данных:

- First Name
- Last Name
- Email

Дополнительные данные пользователь заполняет самостоятельно во время onboarding.

---

# 8. Client

Client — пользователь со стороны организации клиента.

Client принадлежит определённой Organization.

Client получает ограниченный доступ к системе.

Основная задача Client — иметь возможность видеть состояние проекта и разрешённую информацию без необходимости постоянно обращаться к сотрудникам компании.

Client не должен видеть техническую или внутреннюю информацию, если она ему не предназначена.

UX для Client должен быть особенно понятным.

---

# 9. Client Manager

Client Manager также относится к определённой Organization.

Он является более привилегированным клиентским пользователем.

Главное отличие Client Manager:

он может приглашать других сотрудников своей организации в BAIM.

Приглашённые им пользователи получают роль Client.

Таким образом Super Admin не обязан самостоятельно регистрировать каждого сотрудника клиента.

---

# 10. User registration

## Employee registration

Super Admin создаёт Employee.

Не требуется Organization.

Основные поля:

- First Name
- Last Name
- Email

---

## Client registration

Super Admin создаёт пользователя.

Дополнительно необходимо выбрать Organization.

---

## Client Manager registration

Super Admin создаёт пользователя.

Дополнительно необходимо выбрать Organization.

---

# 11. Onboarding

После регистрации пользователь проходит onboarding.

Onboarding зависит от global user role.

---

## Employee onboarding

### Step: Personal Information

Пользователь может заполнить:

- avatar;
- phone number;
- другие личные данные.

### Step: Security

Например:

- change password.

---

## Client onboarding

Client проходит:

1. Personal Information
2. Security

---

## Client Manager onboarding

Client Manager проходит:

1. Personal Information
2. Security
3. Invite

### Invite step

Client Manager может пригласить сотрудников своей Organization.

Приглашённые пользователи получают global role:

Client.

Цель этого механизма — позволить Client Manager самостоятельно создавать клиентских пользователей без участия Super Admin.

---

# 12. Main application navigation

После onboarding пользователи получают доступ к основному приложению.

Планируемые основные страницы:

- Dashboard
- Kanban
- Projects
- Notifications
- Settings

В будущем список может расширяться.

На данный момент полноценно начата реализация Projects.

---

# 13. Projects page

Projects показывает проекты, доступные текущему пользователю.

Обычный пользователь видит только проекты, участником которых он является.

Super Admin видит все проекты.

---

# 14. Project creation

Super Admin может создавать Project.

При создании проекта выбирается Project Type.

Существует минимум два типа:

## Internal Project

Проект предназначен для самой компании.

Client users не участвуют.

## External Project

Проект создаётся для определённой Organization.

В таком проекте могут участвовать клиентские пользователи.

---

# 15. Project participants

В Project можно добавлять Participants.

Participant выбирается из зарегистрированных пользователей.

Каждому участнику назначается project-specific role.

Global user role и project role являются разными понятиями.

Global role определяет тип пользователя в системе.

Project role определяет его функцию внутри конкретного проекта.

---

# 16. Project roles

Существуют следующие project roles:

- Project Manager
- Business Consultant
- Developer
- Client Viewer
- Client Manager

---

# 17. Project role restrictions

Client Viewer и Client Manager могут быть назначены только:

- пользователям с клиентской global role;
- в External Project.

Project Manager, Business Consultant и Developer назначаются Employee users.

Эти ограничения должны учитываться не только backend validation, но и UX.

Интерфейс не должен предлагать пользователю заведомо невозможные варианты.

---

# 18. Project hierarchy

Внутри Project существует иерархия рабочих объектов.

Project
→ Group
→ Milestone
→ Ticket
→ Task
→ Subtask

То есть:

Project содержит Groups.

Group содержит Milestones.

Milestone содержит Tickets.

Ticket содержит Tasks.

Task может содержать Subtasks.

---

# 19. Entity importance

Не все уровни иерархии являются одинаковыми полноценными сущностями.

Project является полноценной сущностью.

Ticket является полноценной сущностью.

Task является полноценной рабочей сущностью.

Subtask является полноценной рабочей сущностью.

Group и Milestone в текущей архитектуре используются прежде всего как organizational hierarchy.

Это влияет на такие функции как History.

---

# 20. Current development state

На данный момент реализованы:

- Projects;
- Groups;
- Milestones;
- Tickets;
- часть связанных с ними интерфейсов.

Некоторые существующие элементы будут пересматриваться.

В частности:

- Attachments;
- History;
- некоторые Project UI;
- некоторые Ticket UI.

---

# 21. Current implementation roadmap

Текущий предполагаемый порядок работы:

1. Tasks
2. Subtasks
3. History
4. Attachments
5. Comments
6. Kanban
7. дальнейшие части системы

Этот roadmap может изменяться.

---

# 22. History concept

История должна быть полноценной частью продукта.

---

# 23. Project History

Project History показывает изменения Project.

История создаётся после сохранения изменений страницы Edit Project.

При нажатии Submit необходимо фиксировать:

- кто изменил;
- когда изменил;
- какие поля изменились;
- old value;
- new value.

Пример записи:

Status
Draft → Active

Target Date
12 September → 20 September

Project Manager
John Doe → Alice Smith

---

# 24. Group and Milestone history

Group и Milestone не имеют собственной отдельной History.

Поэтому изменения:

- Group;
- Milestone

должны отображаться внутри Project History.

История проекта таким образом содержит не только прямые изменения Project, но и структурные изменения его hierarchy.

---

# 25. Ticket History

Ticket имеет собственную History.

В ней должны отображаться изменения Ticket начиная с момента его создания.

Например:

- Ticket created
- Title changed
- Description changed
- Status changed
- Type changed
- Assignment changed
- другие изменения

История должна содержать:

- actor;
- date/time;
- event/change;
- old value;
- new value.

---

# 26. History UX inspiration

Для History можно использовать Azure DevOps как UX reference.

Предполагаемый pattern:

левая часть:
- chronological activity list;

правая часть:
- details выбранного history item.

Когда пользователь выбирает событие слева, справа показывается подробная информация.

Это reference, а не требование копировать Azure DevOps визуально.

Необходимо адаптировать pattern к дизайну BAIM.

---

# 27. Attachments general principle

Физически загружать attachments можно только внутри:

- Task
- Subtask

Project и Ticket не загружают собственные файлы напрямую.

Но они должны агрегировать attachments своих дочерних элементов.

---

# 28. Task Attachments

Task может иметь собственные attachments.

Пользователь может:

- upload;
- download;
- просматривать информацию о файле.

---

# 29. Subtask Attachments

Subtask также может иметь attachments.

---

# 30. Ticket Attachments

Ticket Attachments tab является aggregated view.

Он показывает attachments:

- Tasks Ticket;
- Subtasks этих Tasks.

Если Ticket содержит:

- Task 1
- Task 2
- Task 3
- Task 4

и некоторые Tasks имеют Subtasks, attachments всех этих объектов должны быть доступны в Ticket Attachments.

---

# 31. Project Attachments

Project Attachments является aggregated view всех attachments внутри Project hierarchy.

Файл может находиться глубоко внутри структуры:

Project
→ Group
→ Milestone
→ Ticket
→ Task
→ Subtask
→ Attachment

Project Attachments должен позволить понять происхождение файла.

---

# 32. Attachment context and hierarchy

Для каждого aggregated attachment необходимо показывать context.

Пользователь должен понимать:

- к какой Task относится файл;
- к какой Subtask относится файл, если применимо;
- Ticket;
- Milestone;
- Group;
- Project.

Не обязательно показывать каждый hierarchy level одинаково большим текстом.

Необходимо спроектировать компактный и понятный hierarchy representation.

Например можно использовать:

- breadcrumb;
- hierarchy path;
- nested metadata;
- contextual link;
- combination этих patterns.

Для Task/Subtask необходимо отображать:

- code / number;
- title;
- clickable link.

Пользователь должен иметь возможность перейти к исходной Task/Subtask.

---

# 33. Attachments primary actions

Aggregated Attachments views в Project и Ticket предназначены прежде всего для поиска и просмотра существующих файлов.

Upload доступен только на Task/Subtask.

В Project/Ticket Attachment view пользователь может:

- видеть файл;
- понимать его источник;
- видеть hierarchy;
- перейти к entity;
- download attachment.

---

# 34. Future Comments

После Tasks, Subtasks, History и Attachments планируется Comments.

Comments должны быть спроектированы как часть collaborative workflow.

При проектировании Comments необходимо учитывать разные global roles и project roles.

Особенно необходимо учитывать Client users.

Нельзя автоматически предполагать, что вся внутренняя коммуникация должна быть доступна Client.

Permission model для Comments должен быть определён отдельно.

---

# 35. Future Kanban

Kanban является одной из основных будущих страниц.

Она должна использовать существующие сущности и status workflow.

Kanban не должен выглядеть как отдельное приложение.

Он должен продолжать BAIM design language.

---

# 36. Design consistency

Все новые страницы должны соответствовать существующему BAIM.

Нельзя проектировать каждую страницу независимо.

Если пользователь видит определённый pattern на Project page, аналогичная задача на Ticket/Task должна использовать тот же pattern, если нет веской UX-причины его изменить.

Consistency должна соблюдаться в:

- page headers;
- buttons;
- border radius;
- spacing;
- typography;
- forms;
- tabs;
- dropdowns;
- date pickers;
- cards;
- tables;
- status representation;
- entity codes;
- breadcrumbs;
- modals;
- drawers;
- empty states;
- loading states;
- filters;
- search;
- responsive behavior;
- hover states;
- focus states;
- disabled states;
- error states.

---

# 37. Existing visual direction

Текущий BAIM использует преимущественно:

- orange;
- white;
- gray.

Orange является основным accent color.

Новые страницы должны продолжать эту visual identity.

Нельзя самовольно переводить продукт в:

- blue SaaS style;
- purple AI style;
- dark enterprise style;
- random multicolor style.

Цвет должен использоваться осознанно.

Orange не означает, что каждый элемент интерфейса должен быть оранжевым.

Accent должен помогать hierarchy.

---

# 38. Responsive design

Все страницы BAIM должны быть responsive.

Нельзя проектировать desktop layout без понимания того, как он трансформируется на меньших размерах.

Необходимо учитывать как минимум:

- wide desktop;
- normal desktop;
- laptop;
- tablet;
- mobile.

Responsive design не означает просто уменьшение размеров.

Необходимо определять:

- какие элементы переходят на новую строку;
- что становится collapsible;
- что перемещается;
- где table превращается в другой representation;
- где sidebar превращается в drawer;
- какие actions остаются видимыми;
- какие уходят в overflow menu.

---

# 39. Product design objective

Главная цель дизайна BAIM:

создать professional, scalable, intuitive B2B product, которым одинаково комфортно пользоваться сотрудникам компании и клиентам.

Интерфейс должен помогать пользователю понять:

- где он находится;
- что происходит;
- что важно;
- что он может сделать;
- куда перейти дальше.

Пользователь не должен изучать интерфейс.

Интерфейс должен объяснять себя сам.